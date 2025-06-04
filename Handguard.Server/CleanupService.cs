namespace Handguard.Server;

public class CleanupService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = DateTime.Today.AddDays(1).AddHours(1); // 1 AM

            var delay = nextRun - now;
            await Task.Delay(delay, stoppingToken);

            FileStorage.CleanupOldFiles();
        }
    }
}
