namespace Handguard.Lib.Models
{
    public class FileInfoResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public long Size { get; set; }
    }
}
