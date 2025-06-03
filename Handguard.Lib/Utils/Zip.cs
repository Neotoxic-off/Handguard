using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handguard.Lib.Utils
{
    public static class Zip
    {
        public static string? Compress(string folderPath, string outputDirectory)
        {
            string? path = null;

            if (!Directory.Exists(folderPath))
            {
                path = Path.Combine(outputDirectory, $"{Path.GetFileName(folderPath)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip");

                ZipFile.CreateFromDirectory(folderPath, path, CompressionLevel.Fastest, true);

                return path;

            }

            return null;
        }
    }
}
