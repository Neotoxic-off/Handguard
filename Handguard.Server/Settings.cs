namespace Handguard.Server
{
    public static class Settings
    {
        public static readonly string StorageDir = Path.Combine(AppContext.BaseDirectory, "storage");
        public static readonly long MaxUploadSize = 30L * 1024 * 1024 * 1024;
    }
}
