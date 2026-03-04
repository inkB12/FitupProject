using FitupProject.BLL.Commons;
using FitupProject.BLL.DTOs.PTRegister;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FitupProject.BLL.Services
{
    public class AdminPTService : IAdminPTService
    {
        private readonly IUnitOfWork _uow;

        public AdminPTService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PagedResult<AdminPTListItemDto>> GetPtsAsync(string? status, int pageIndex, int pageSize)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var ptRepo = _uow.GetRepository<PT>();
            var q = ptRepo.Entities.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<VerificationStatus>(status, true, out var st))
                    throw new Exception("Invalid status. Allowed: Pending/Verified/Rejected.");

                q = q.Where(x => x.VerificationStatus == st);
            }

            var total = await q.LongCountAsync();

            var items = await q
                .OrderByDescending(x => x.SubmittedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AdminPTListItemDto
                {
                    PTId = x.Id,
                    AccountId = x.AccountId,
                    DisplayName = x.DisplayName,
                    VerificationStatus = x.VerificationStatus.ToString(),
                    SubmittedAt = x.SubmittedAt
                })
                .ToListAsync();

            return new PagedResult<AdminPTListItemDto>
            {
                Items = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public async Task<AdminPTDetailDto> GetPtDetailAsync(string ptId)
        {
            var ptRepo = _uow.GetRepository<PT>();

            var pt = await ptRepo.Entities
                .AsNoTracking()
                .Include(x => x.CertificationFiles)
                .FirstOrDefaultAsync(x => x.Id == ptId)
                ?? throw new Exception("PT profile not found.");

            List<string> FromJson(string json)
                => string.IsNullOrWhiteSpace(json)
                    ? new()
                    : (JsonSerializer.Deserialize<List<string>>(json) ?? new());

            return new AdminPTDetailDto
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

        public async Task ApproveAsync(string ptId, string reviewerAccountId)
        {
            var ptRepo = _uow.GetRepository<PT>();
            var accRepo = _uow.GetRepository<Account>();
            var logRepo = _uow.GetRepository<PTReviewLog>();

            var pt = await ptRepo.Entities.FirstOrDefaultAsync(x => x.Id == ptId)
                     ?? throw new Exception("PT profile not found.");

            if (pt.VerificationStatus != VerificationStatus.Pending)
                throw new Exception("Only Pending PT can be approved.");

            var acc = await accRepo.Entities.FirstOrDefaultAsync(x => x.Id == pt.AccountId)
                      ?? throw new Exception("Account not found.");

            var now = DateTimeOffset.UtcNow;

            pt.VerificationStatus = VerificationStatus.Verified;
            pt.ReviewedAt = now;
            pt.ReviewedBy = reviewerAccountId;
            pt.RejectedReason = null;

            ptRepo.Update(pt);

            // chuyển role
            acc.Role = AccountRole.PT;
            accRepo.Update(acc);

            await logRepo.AddAsync(new PTReviewLog
            {
                PTId = pt.Id,
                Action = PTReviewAction.Approved,
                ActorAccountId = reviewerAccountId,
                ActionAt = now,
                SnapshotJson = JsonSerializer.Serialize(new { pt.Id, pt.AccountId, pt.DisplayName, pt.VerificationStatus, ApprovedAt = now })
            });

            await _uow.SaveAsync();
        }

        public async Task RejectAsync(string ptId, string reviewerAccountId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new Exception("Reject reason is required.");

            var ptRepo = _uow.GetRepository<PT>();
            var logRepo = _uow.GetRepository<PTReviewLog>();

            var pt = await ptRepo.Entities.FirstOrDefaultAsync(x => x.Id == ptId)
                     ?? throw new Exception("PT profile not found.");

            if (pt.VerificationStatus != VerificationStatus.Pending)
                throw new Exception("Only Pending PT can be rejected.");

            var now = DateTimeOffset.UtcNow;

            pt.VerificationStatus = VerificationStatus.Rejected;
            pt.ReviewedAt = now;
            pt.ReviewedBy = reviewerAccountId;
            pt.RejectedReason = reason.Trim();

            ptRepo.Update(pt);

            await logRepo.AddAsync(new PTReviewLog
            {
                PTId = pt.Id,
                Action = PTReviewAction.Rejected,
                ActorAccountId = reviewerAccountId,
                ActionAt = now,
                Reason = pt.RejectedReason,
                SnapshotJson = JsonSerializer.Serialize(new { pt.Id, pt.AccountId, pt.DisplayName, pt.VerificationStatus, RejectedAt = now, pt.RejectedReason })
            });

            await _uow.SaveAsync();
        }
    }
}
