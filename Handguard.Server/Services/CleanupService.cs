using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Handguard.Server
{
    public partial class CleanupService : BackgroundService
    {
        private static readonly object FileLock = new object();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime now = DateTime.UtcNow;
                DateTime nextRun = DateTime.UtcNow.Date.AddDays(1).AddHours(1);
                TimeSpan delay = nextRun - now;

                await Task.Delay(delay, stoppingToken);

                lock (FileLock)
                {
                    if (Directory.Exists(Settings.StorageDir))
                    {
                        string[] files = Directory.GetFiles(Settings.StorageDir);
                        foreach (string file in files)
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch
                            {
                                // log and continue
                            }
                        }
                    }
                }
            }
        }

        public static void EnterFileCriticalSection(Action action)
        {
            lock (FileLock)
            {
                action();
            }
        }

        public static async Task<T> EnterFileCriticalSectionAsync<T>(Func<Task<T>> action)
        {
            lock (FileLock)
            {
                return action().Result;
            }
        }
    }
}
