using FitupProject.BLL.DTOs.DashBoard;
using FitupProject.BLL.Interfaces;
using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitupProject.BLL.Services
{
    public class DashBoardService : IDashBoardService
    {
        private readonly IUnitOfWork _uow;
        public DashBoardService(IUnitOfWork uow)
        {
           _uow = uow;
        }
        public async Task<GetSummaryResponse> GetSummaryAsync(GetSummaryRequest request)
        {
            var userRepo = _uow.GetRepository<Account>();
            var bookingRepo = _uow.GetRepository<Booking>();
            var pointRepo = _uow.GetRepository<Payment>();

            var today = DateTime.UtcNow.Date;
            var startDefault = new DateTimeOffset(today, TimeSpan.Zero);
            var endDefault = startDefault.AddDays(1);

            DateTimeOffset from = DateTimeOffset.TryParse(request?.FromDate,
                          System.Globalization.CultureInfo.InvariantCulture,
                          System.Globalization.DateTimeStyles.AssumeUniversal |
                          System.Globalization.DateTimeStyles.AdjustToUniversal,
                          out var parsedFrom)
                      ? parsedFrom
                      : startDefault;

            DateTimeOffset to = DateTimeOffset.TryParse(request?.ToDate,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.AssumeUniversal |
                                    System.Globalization.DateTimeStyles.AdjustToUniversal,
                                    out var parsedTo)
                                ? parsedTo
                                : endDefault;

            var response = new GetSummaryResponse()
            {
                TotalUsers = await userRepo.Entities.CountAsync(x => x.Role == AccountRole.User),

                TodayBookings = await bookingRepo.Entities
                    .CountAsync(b => b.CreatedAt >= startDefault && b.CreatedAt < endDefault),

                PendingPTs = await userRepo.Entities
                    .CountAsync(x => x.Role == AccountRole.PT
                                && x.Status == AccountStatus.PendingVerification),

                PointRevenue = await pointRepo.Entities
                    .Where(p => p.Status == PaymentStatus.Success
                             && p.PaidAt >= from
                             && p.PaidAt < to)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0
            };

            return response;
        }
    }
}
