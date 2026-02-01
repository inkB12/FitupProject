using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.BackgroundJobs
{
    public class PaymentExpiryHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentExpiryHostedService> _logger;

        public PaymentExpiryHostedService(IServiceScopeFactory scopeFactory, ILogger<PaymentExpiryHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var repo = uow.GetRepository<Payment>();
                    var now = DateTimeOffset.UtcNow;

                    // lấy các payment pending đã quá hạn
                    var expired = await repo.Entities
                        .Where(p => p.Status == PaymentStatus.Pending
                                    && p.ExpiredAt != null
                                    && p.ExpiredAt < now)
                        .ToListAsync(stoppingToken);

                    if (expired.Count > 0)
                    {
                        foreach (var p in expired)
                        {
                            p.Status = PaymentStatus.Cancelled;
                            repo.Update(p);
                        }

                        await uow.SaveAsync();
                        _logger.LogInformation("Cancelled {Count} expired pending payments", expired.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PaymentExpiryHostedService error");
                }

                // chạy mỗi 60s
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}
