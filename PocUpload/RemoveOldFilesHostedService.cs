using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PocUpload
{
    public class RemoveOldFilesHostedService : BackgroundService
    {
        private DateTime _nextRun;
        private readonly CrontabSchedule _schedule;
        private readonly ILogger<RemoveOldFilesHostedService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private string Schedule => "* * * * *"; // Runs every minute












        public RemoveOldFilesHostedService(ILogger<RemoveOldFilesHostedService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _schedule = CrontabSchedule.Parse(Schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = false });
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                var now = DateTime.Now;
                if (now > _nextRun)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        _logger.LogInformation($"Process: {DateTime.Now:F}");
                        var manageFileRepository = scope.ServiceProvider.GetRequiredService<IManageFileRepository>();
                        manageFileRepository.RemoveOldFiles();
                    }
                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }
                await Task.Delay(5000, stoppingToken);
            }
            while (!stoppingToken.IsCancellationRequested);
        }
    }
}
