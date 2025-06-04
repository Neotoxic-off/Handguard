using System.Security.Cryptography;

namespace Handguard.Server;

public static class FileStorage
{
    private static readonly string DirectoryPath = Path.Combine(AppContext.BaseDirectory, "Files");

    static FileStorage()
    {
        Directory.CreateDirectory(DirectoryPath);
    }

    public static async Task<string> SaveAsync(IFormFile file, string pass)
    {
        string id = Guid.NewGuid().ToString("N");
        string filePath = Path.Combine(DirectoryPath, id);
        string metaPath = filePath + ".meta";

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        await File.WriteAllTextAsync(metaPath, pass ?? "");

        return id;
    }

    public static (Stream Stream, string FileName, string ContentType)? Get(string id, string pass)
    {
        string filePath = Path.Combine(DirectoryPath, id);
        string metaPath = filePath + ".meta";

        if (!File.Exists(filePath) || !File.Exists(metaPath)) return null;

        var storedPass = File.ReadAllText(metaPath);
        if (storedPass != (pass ?? "")) return null;

        return (File.OpenRead(filePath), Path.GetFileName(filePath), "application/octet-stream");
    }

    public static void CleanupOldFiles()
    {
        foreach (var file in Directory.GetFiles(DirectoryPath))
        {
            if (File.GetCreationTime(file).Date < DateTime.Now.Date)
            {
                File.Delete(file);
                var meta = file + ".meta";
                if (File.Exists(meta)) File.Delete(meta);
            }
        }
    }
}
