using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Handguard.Server
{
    public static class FileStorage
    {
        static FileStorage()
        {
            if (!Directory.Exists(Settings.StorageDir))
                Directory.CreateDirectory(Settings.StorageDir);
        }

        public static async Task<string> SaveAsync(IFormFile file, string password)
        {
            return await CleanupService.EnterFileCriticalSectionAsync(async () =>
            {
                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                string rawId = timestamp + Guid.NewGuid().ToString("N")[..8];
                string id = Sha256Hex(rawId)[..32];

                string filePath = Path.Combine(Settings.StorageDir, id + ".dat");
                string metadataPath = Path.Combine(Settings.StorageDir, id + ".meta");

                using FileStream fs = new FileStream(filePath, FileMode.CreateNew);
                await file.CopyToAsync(fs);
                await File.WriteAllTextAsync(metadataPath, password);

                return id;
            });
        }

        public static (Stream Stream, string FileName, string ContentType)? Get(string id, string password)
        {
            (Stream, string, string)? result = null;

            CleanupService.EnterFileCriticalSection(() =>
            {
                if (!IsValidId(id))
                    return;

                string filePath = Path.Combine(Settings.StorageDir, id + ".dat");
                string metadataPath = Path.Combine(Settings.StorageDir, id + ".meta");

                if (!File.Exists(filePath) || !File.Exists(metadataPath))
                    return;

                string storedPass = File.ReadAllText(metadataPath);
                if (storedPass != password)
                    return;

                FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                result = (stream, id + ".bin", "application/octet-stream");
            });

            return result;
        }

        private static bool IsValidId(string id)
        {
            return id.All(c => char.IsLetterOrDigit(c));
        }

        private static string Sha256Hex(string input)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
