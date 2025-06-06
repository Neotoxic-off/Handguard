using Newtonsoft.Json;

namespace Handguard.Server.Models
{
    public class FileMetadata
    {
        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;

        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonProperty("contentType")]
        public string ContentType { get; set; } = "application/octet-stream";

        [JsonProperty("uploadDate")]
        public DateTime UploadDate { get; set; }

        [JsonProperty("fileSize")]
        public long FileSize { get; set; }
    }
}
