using ACI.Data;
using Microsoft.EntityFrameworkCore;

namespace ACI.Service
{
    public class EmailLogCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<EmailLogCleanupService> _logger;

        public EmailLogCleanupService(IServiceProvider services, ILogger<EmailLogCleanupService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailLogCleanupService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldLogsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing EmailLogCleanupService.");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        private async Task CleanupOldLogsAsync()
        {
            using (var scope = _services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                var thresholdDate = DateTime.UtcNow.AddMonths(-3);
                
                var oldLogs = dbContext.EmailLogs.Where(e => e.CreatedDate < thresholdDate);
                if (await oldLogs.AnyAsync())
                {
                    dbContext.EmailLogs.RemoveRange(oldLogs);
                    int deletedCount = await dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Cleaned up {deletedCount} email logs older than 3 months.");
                }
                else
                {
                    _logger.LogInformation("No old email logs to clean up.");
                }
            }
        }
    }
}
