using FitupProject.BLL.DTOs.PTRegister;
using FitupProject.BLL.DTOs.PTs;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FitupProject.BLL.Services
{
    public class PTService : IPTService
    {
        private const int MaxCertFiles = 10;
        private readonly IUnitOfWork _uow;

        public PTService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PTProfileResponse?> GetProfileAsync(string accountId)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var pt = await ptRepo.Entities
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.AccountId == accountId);

            if (pt == null) return null;

            return new PTProfileResponse
            {
                Id = pt.Id,
                AccountId = pt.AccountId,
                Email = pt.Account?.Email ?? string.Empty,
                Phone = pt.Account?.Phone,
                DisplayName = pt.DisplayName,
                Bio = pt.Bio,
                PricePerHour = pt.PricePerHour,
                Rating = pt.Rating,
                VerificationStatus = pt.VerificationStatus.ToString()
            };
        }

        public async Task<PTProfileResponse?> GetPTByIdAsync(string ptId)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var pt = await ptRepo.Entities
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.Id == ptId && p.VerificationStatus == VerificationStatus.Verified);

            if (pt == null) return null;

            return new PTProfileResponse
            {
                Id = pt.Id,
                AccountId = pt.AccountId,
                Email = pt.Account?.Email ?? string.Empty,
                Phone = pt.Account?.Phone,
                DisplayName = pt.DisplayName,
                Bio = pt.Bio,
                PricePerHour = pt.PricePerHour,
                Rating = pt.Rating,
                VerificationStatus = pt.VerificationStatus.ToString()
            };
        }

        public async Task<IEnumerable<PTListItemResponse>> GetAllPTsAsync(PTFilterRequest? filter = null)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var query = ptRepo.Entities
                .Where(p => p.VerificationStatus == VerificationStatus.Verified);

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Name))
                {
                    query = query.Where(p => p.DisplayName.ToLower().Contains(filter.Name.ToLower()));
                }

                if (filter.MinPrice.HasValue)
                {
                    query = query.Where(p => p.PricePerHour >= filter.MinPrice.Value);
                }

                if (filter.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.PricePerHour <= filter.MaxPrice.Value);
                }
            }

            return await query
                .OrderByDescending(p => p.Rating)
                .Select(p => new PTListItemResponse
                {
                    Id = p.Id,
                    DisplayName = p.DisplayName,
                    Bio = p.Bio,
                    PricePerHour = p.PricePerHour,
                    Rating = p.Rating
                })
                .ToListAsync();
        }

        public async Task<PTMeResponse> RegisterAsync(string accountId, PTRegisterRequest req)
        {
            ValidateRegister(req);

            var accRepo = _uow.GetRepository<Account>();
            var ptRepo = _uow.GetRepository<PT>();
            var fileRepo = _uow.GetRepository<PTCertificationFile>();
            var logRepo = _uow.GetRepository<PTReviewLog>();

            var account = await accRepo.Entities.FirstOrDefaultAsync(x => x.Id == accountId)
                          ?? throw new Exception("Account not found.");

            // chỉ User được đăng ký PT
            if (account.Role != AccountRole.User)
                throw new Exception("Only User role can register PT profile.");

            var now = DateTimeOffset.UtcNow;

            var pt = await ptRepo.Entities
                .Include(x => x.CertificationFiles)
                .FirstOrDefaultAsync(x => x.AccountId == accountId);

            // helper serialize JSON string
            string ToJson(List<string>? items) => JsonSerializer.Serialize(NormalizeList(items));

            if (pt == null)
            {
                pt = new PT
                {
                    AccountId = accountId,
                    DisplayName = req.DisplayName.Trim(),
                    Bio = req.Bio?.Trim(),

                    ExperienceYears = req.ExperienceYears,
                    HourlyPointRate = req.HourlyPointRate,
                    Location = req.Location?.Trim(),

                    CertificationsJson = ToJson(req.Certifications),
                    SpecialtiesJson = ToJson(req.Specialties),
                    LanguagesJson = ToJson(req.Languages),

                    VerificationStatus = VerificationStatus.Pending,
                    SubmittedAt = now,

                    // compatibility: code cũ có thể đang dùng PricePerHour
                    PricePerHour = req.HourlyPointRate
                };

                await ptRepo.AddAsync(pt);

                if (req.CertificationFiles?.Any() == true)
                {
                    foreach (var f in req.CertificationFiles)
                    {
                        await fileRepo.AddAsync(new PTCertificationFile
                        {
                            PTId = pt.Id,
                            FileName = f.FileName.Trim(),
                            FileUrl = f.FileUrl.Trim(),
                            ContentType = f.ContentType?.Trim(),
                            FileSize = f.FileSize,
                            UploadedAt = now
                        });
                    }
                }

                await logRepo.AddAsync(new PTReviewLog
                {
                    PTId = pt.Id,
                    Action = PTReviewAction.Submitted,
                    ActorAccountId = accountId,
                    ActionAt = now,
                    Reason = "Submitted",
                    SnapshotJson = BuildSnapshot(pt)
                });

                await _uow.SaveAsync();
                return await GetMeAsync(accountId);
            }

            // đã có hồ sơ
            if (pt.VerificationStatus == VerificationStatus.Pending || pt.VerificationStatus == VerificationStatus.Verified)
                throw new Exception("PT profile already exists and cannot be resubmitted in current status.");

            // Rejected => allow resubmit
            if (pt.VerificationStatus == VerificationStatus.Rejected)
            {
                pt.DisplayName = req.DisplayName.Trim();
                pt.Bio = req.Bio?.Trim();

                pt.ExperienceYears = req.ExperienceYears;
                pt.HourlyPointRate = req.HourlyPointRate;
                pt.Location = req.Location?.Trim();

                pt.CertificationsJson = ToJson(req.Certifications);
                pt.SpecialtiesJson = ToJson(req.Specialties);
                pt.LanguagesJson = ToJson(req.Languages);

                pt.VerificationStatus = VerificationStatus.Pending;
                pt.SubmittedAt = now;

                pt.ReviewedAt = null;
                pt.ReviewedBy = null;
                pt.RejectedReason = null;

                pt.PricePerHour = req.HourlyPointRate;

                ptRepo.Update(pt);

                // replace files
                if (pt.CertificationFiles?.Any() == true)
                    fileRepo.DeleteRange(pt.CertificationFiles);

                if (req.CertificationFiles?.Any() == true)
                {
                    foreach (var f in req.CertificationFiles)
                    {
                        await fileRepo.AddAsync(new PTCertificationFile
                        {
                            PTId = pt.Id,
                            FileName = f.FileName.Trim(),
                            FileUrl = f.FileUrl.Trim(),
                            ContentType = f.ContentType?.Trim(),
                            FileSize = f.FileSize,
                            UploadedAt = now
                        });
                    }
                }

                await logRepo.AddAsync(new PTReviewLog
                {
                    PTId = pt.Id,
                    Action = PTReviewAction.Resubmitted,
                    ActorAccountId = accountId,
                    ActionAt = now,
                    Reason = "Resubmitted after rejection",
                    SnapshotJson = BuildSnapshot(pt)
                });

                await _uow.SaveAsync();
                return await GetMeAsync(accountId);
            }

            throw new Exception("Invalid PT state.");
        }

        public async Task<PTMeResponse> GetMeAsync(string accountId)
        {
            var ptRepo = _uow.GetRepository<PT>();

            var pt = await ptRepo.Entities
                .Include(x => x.CertificationFiles)
                .FirstOrDefaultAsync(x => x.AccountId == accountId)
                ?? throw new Exception("PT profile not found.");

            List<string> FromJson(string json)
                => string.IsNullOrWhiteSpace(json)
                    ? new()
                    : (JsonSerializer.Deserialize<List<string>>(json) ?? new());

            return new PTMeResponse
            {
                PTId = pt.Id,
                AccountId = pt.AccountId,

                DisplayName = pt.DisplayName,
                Bio = pt.Bio,

                ExperienceYears = pt.ExperienceYears,
                HourlyPointRate = pt.HourlyPointRate,
                Location = pt.Location,

                Certifications = FromJson(pt.CertificationsJson),
                Specialties = FromJson(pt.SpecialtiesJson),
                Languages = FromJson(pt.LanguagesJson),

                VerificationStatus = pt.VerificationStatus.ToString(),
                SubmittedAt = pt.SubmittedAt,
                ReviewedAt = pt.ReviewedAt,
                ReviewedBy = pt.ReviewedBy,
                RejectedReason = pt.RejectedReason,

                CertificationFiles = (pt.CertificationFiles ?? new List<PTCertificationFile>())
                    .OrderByDescending(x => x.UploadedAt)
                    .Select(x => new PTCertificationFileDto
                    {
                        FileName = x.FileName,
                        FileUrl = x.FileUrl,
                        ContentType = x.ContentType,
                        FileSize = x.FileSize
                    })
                    .ToList()
            };
        }

        private static void ValidateRegister(PTRegisterRequest req)
        {
            if (req.CertificationFiles != null && req.CertificationFiles.Count > MaxCertFiles)
                throw new Exception($"Max {MaxCertFiles} certification files allowed.");

            ValidateStringList(req.Certifications, "Certifications", 30, 60);
            ValidateStringList(req.Specialties, "Specialties", 30, 60);
            ValidateStringList(req.Languages, "Languages", 20, 60);
        }

        private static void ValidateStringList(List<string>? items, string field, int maxCount, int maxLen)
        {
            if (items == null) return;
            if (items.Count > maxCount) throw new Exception($"{field} max {maxCount} items.");

            foreach (var s in items)
            {
                if (string.IsNullOrWhiteSpace(s)) throw new Exception($"{field} contains empty item.");
                if (s.Trim().Length > maxLen) throw new Exception($"{field} item too long (max {maxLen}).");
            }
        }

        private static List<string> NormalizeList(List<string>? items)
            => items?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
               ?? new();

        private static string BuildSnapshot(PT pt)
        {
            var obj = new
            {
                pt.DisplayName,
                pt.Bio,
                pt.ExperienceYears,
                pt.HourlyPointRate,
                pt.Location,
                pt.CertificationsJson,
                pt.SpecialtiesJson,
                pt.LanguagesJson,
                pt.VerificationStatus,
                pt.SubmittedAt
            };
            return JsonSerializer.Serialize(obj);
        }
    }
}
