using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ACI.BackgroundServices
{
    public class PeriodicLoggingService : BackgroundService
    {
        private readonly ILogger<PeriodicLoggingService> _logger;

        public PeriodicLoggingService(ILogger<PeriodicLoggingService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("App Status: Running (5 Minute Periodic Log)");
                // Wait for 5 minutes before logging again
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
