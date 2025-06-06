using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Handguard.Server.Services
{
    public static class SecurityService
    {
        private static readonly Regex ValidIdPattern = new Regex("^[a-fA-F0-9]{32}$", RegexOptions.Compiled);

        public static string GenerateSecurePassword()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        }

        public static string GenerateSecureId()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            string guid = Guid.NewGuid().ToString("N")[..16];
            string combined = timestamp + guid;
            return ComputeSha256Hash(combined)[..32];
        }

        public static bool IsValidId(string id)
        {
            return !string.IsNullOrEmpty(id) && ValidIdPattern.IsMatch(id);
        }

        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "unknown";

            string invalidChars = new string(Path.GetInvalidFileNameChars());
            string sanitized = fileName;

            foreach (char c in invalidChars)
                sanitized = sanitized.Replace(c, '_');

            return sanitized.Length > 255 ? sanitized[..255] : sanitized;
        }

        private static string ComputeSha256Hash(string input)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
