using FitupProject.Core.Commons.Enums;
using FitupProject.Core.Entities;
using FitupProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.Services
{
    public class PendingAccountCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public PendingAccountCleanupService(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var repo = uow.GetRepository<Account>();

                    var now = DateTimeOffset.UtcNow;

                    // lấy list expired pending accounts
                    var expired = await repo.Entities
                        .Where(a => a.Status == AccountStatus.PendingVerification
                                 && a.EmailOtpExpiresAt != null
                                 && a.EmailOtpExpiresAt < now)
                        .ToListAsync(stoppingToken);

                    if (expired.Count > 0)
                    {
                        repo.DeleteRange(expired);
                        await uow.SaveAsync();
                    }
                }
                catch
                {
                    // có thể log
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
