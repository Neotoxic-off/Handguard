using Microsoft.Extensions.Hosting;

namespace Handguard.Server.Services
{
    public class CleanupService : BackgroundService
    {
        private readonly FileStorageService _fileStorageService;

        public CleanupService(FileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime nextRun = GetNextCleanupTime();
                TimeSpan delay = nextRun - DateTime.UtcNow;

                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, stoppingToken);

                await _fileStorageService.DeleteExpiredFilesAsync();
            }
        }

        private static DateTime GetNextCleanupTime()
        {
            DateTime now = DateTime.UtcNow;
            return now.Date.AddDays(1).AddHours(1);
        }
    }
}