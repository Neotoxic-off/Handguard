public static class FileStorage
{
    private static readonly string StorageDir = Path.Combine(AppContext.BaseDirectory, "storage");

    static FileStorage()
    {
        if (!Directory.Exists(StorageDir))
            Directory.CreateDirectory(StorageDir);
    }

    public static async Task<string> SaveAsync(IFormFile file, string password)
    {
        string id = Guid.NewGuid().ToString("N");
        string filePath = Path.Combine(StorageDir, id + ".dat");
        string metadataPath = Path.Combine(StorageDir, id + ".meta");

        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fs);
        }

        await File.WriteAllTextAsync(metadataPath, password);
        return id;
    }

    public static (Stream Stream, string FileName, string ContentType)? Get(string id, string password)
    {
        if (!IsValidId(id))
            return null;

        string filePath = Path.Combine(StorageDir, id + ".dat");
        string metadataPath = Path.Combine(StorageDir, id + ".meta");

        if (!File.Exists(filePath) || !File.Exists(metadataPath))
            return null;

        string storedPass = File.ReadAllText(metadataPath);
        if (storedPass != password)
            return null;

        FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return (stream, id + ".bin", "application/octet-stream");
    }

    private static bool IsValidId(string id)
    {
        return id.All(c => char.IsLetterOrDigit(c) || c == '-');
    }
}
