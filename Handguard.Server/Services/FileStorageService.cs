using Handguard.Server.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Handguard.Server.Services
{
    public class FileStorageService
    {
        private readonly string _storageDirectory;
        private readonly SemaphoreSlim _fileLock;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks;

        public FileStorageService(string storageDirectory)
        {
            _storageDirectory = storageDirectory;
            _fileLock = new SemaphoreSlim(1, 1);
            _fileLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

            if (!Directory.Exists(_storageDirectory))
                Directory.CreateDirectory(_storageDirectory);
        }

        public async Task<UploadResponse> SaveFileAsync(IFormFile file)
        {
            string id = SecurityService.GenerateSecureId();
            string password = SecurityService.GenerateSecurePassword();

            FileMetadata metadata = new FileMetadata
            {
                Password = password,
                FileName = SecurityService.SanitizeFileName(file.FileName),
                ContentType = file.ContentType ?? "application/octet-stream",
                UploadDate = DateTime.UtcNow,
                FileSize = file.Length
            };

            await ExecuteWithFileLock(id, async () =>
            {
                await SaveFileDataAsync(id, file);
                await SaveFileMetadataAsync(id, metadata);
            });

            return new UploadResponse { Id = id, Password = password };
        }

        public async Task<FileDownloadResult?> GetFileAsync(string id, string password)
        {
            if (!SecurityService.IsValidId(id))
                return null;

            return await ExecuteWithFileLock<FileDownloadResult?>(id, async () =>
            {
                FileMetadata? metadata = await LoadFileMetadataAsync(id);
                if (metadata == null || metadata.Password != password)
                    return null;

                string dataPath = GetDataPath(id);
                if (!File.Exists(dataPath))
                    return null;

                FileStream stream = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return new FileDownloadResult(stream, metadata.FileName, metadata.ContentType);
            });
        }

        public async Task DeleteExpiredFilesAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                await DeleteExpiredFilesInternal();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task DeleteExpiredFilesInternal()
        {
            if (!Directory.Exists(_storageDirectory))
                return;

            string[] metadataFiles = Directory.GetFiles(_storageDirectory, "*.meta");
            DateTime cutoff = DateTime.UtcNow.AddDays(-1);

            foreach (string metaFile in metadataFiles)
            {
                try
                {
                    string id = Path.GetFileNameWithoutExtension(metaFile);
                    FileMetadata? metadata = await LoadFileMetadataAsync(id);

                    if (metadata != null && metadata.UploadDate < cutoff)
                    {
                        DeleteFilesByPrefix(id);
                    }
                }
                catch
                {
                    DeleteFilesByPrefix(Path.GetFileNameWithoutExtension(metaFile));
                }
            }
        }

        private void DeleteFilesByPrefix(string id)
        {
            try
            {
                string dataPath = GetDataPath(id);
                string metaPath = GetMetadataPath(id);

                if (File.Exists(dataPath))
                    File.Delete(dataPath);
                if (File.Exists(metaPath))
                    File.Delete(metaPath);
            }
            catch
            {
            }
        }

        private async Task<T> ExecuteWithFileLock<T>(string id, Func<Task<T>> action)
        {
            SemaphoreSlim fileLock = _fileLocks.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));

            await fileLock.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                fileLock.Release();
            }
        }

        private async Task ExecuteWithFileLock(string id, Func<Task> action)
        {
            SemaphoreSlim fileLock = _fileLocks.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));

            await fileLock.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                fileLock.Release();
            }
        }

        private async Task SaveFileDataAsync(string id, IFormFile file)
        {
            string dataPath = GetDataPath(id);
            using FileStream stream = new FileStream(dataPath, FileMode.CreateNew, FileAccess.Write);
            await file.CopyToAsync(stream);
        }

        private async Task SaveFileMetadataAsync(string id, FileMetadata metadata)
        {
            string metaPath = GetMetadataPath(id);
            string json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            await File.WriteAllTextAsync(metaPath, json);
        }

        private async Task<FileMetadata?> LoadFileMetadataAsync(string id)
        {
            string metaPath = GetMetadataPath(id);
            if (!File.Exists(metaPath))
                return null;

            try
            {
                string json = await File.ReadAllTextAsync(metaPath);
                return JsonConvert.DeserializeObject<FileMetadata>(json);
            }
            catch
            {
                return null;
            }
        }

        private string GetDataPath(string id) => Path.Combine(_storageDirectory, $"{id}.dat");
        private string GetMetadataPath(string id) => Path.Combine(_storageDirectory, $"{id}.meta");
    }
}