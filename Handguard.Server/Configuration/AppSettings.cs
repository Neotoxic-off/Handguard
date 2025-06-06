namespace Handguard.Server.Configuration
{
    public class AppSettings
    {
        public string StorageDirectory { get; set; } = "Storage";
        public int CleanupHourUtc { get; set; } = 1;
        public int FileRetentionDays { get; set; } = 1;
    }
}
