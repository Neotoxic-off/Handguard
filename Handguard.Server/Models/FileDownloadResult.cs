namespace Handguard.Server.Models
{
    public class FileDownloadResult
    {
        public Stream Stream { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long Length { get; set; }

        public FileDownloadResult(Stream stream, string fileName, string contentType, long length)
        {
            Stream = stream;
            FileName = fileName;
            ContentType = contentType;
            Length = length;
        }
    }
}