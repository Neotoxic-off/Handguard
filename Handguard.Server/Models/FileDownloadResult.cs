namespace Handguard.Server.Models
{
    public class FileDownloadResult
    {
        public Stream Stream { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }

        public FileDownloadResult(Stream stream, string fileName, string contentType)
        {
            Stream = stream;
            FileName = fileName;
            ContentType = contentType;
        }
    }
}