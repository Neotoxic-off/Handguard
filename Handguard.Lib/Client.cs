using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Handguard.Lib
{
    public static class Client
    {
        private const int MinBufferSize = 8192;
        private const int MaxBufferSize = 262144;

        public static async Task<string?> UploadSecureAsync(string filePath, string serverUrl, Action<long>? progressCallback = null)
        {
            using HttpClient httpClient = new HttpClient();
            using MultipartFormDataContent form = new MultipartFormDataContent();

            using FileStream fileStream = File.OpenRead(filePath);
            using StreamContent rawContent = new StreamContent(fileStream);
            using StreamWithProgressContent uploadContent = new StreamWithProgressContent(rawContent, progressCallback);

            uploadContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            form.Add(uploadContent, "file", Path.GetFileName(filePath));

            using HttpResponseMessage response = await httpClient.PostAsync($"{serverUrl}/upload", form);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task DownloadSecureAsync(string id, string password, string serverUrl, string saveToDir, Action<long>? progressCallback = null)
        {
            string url = $"{serverUrl}/download?id={WebUtility.UrlEncode(id)}&pass={WebUtility.UrlEncode(password)}";

            using HttpClient httpClient = new HttpClient();
            using HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            string? fileName = null;

            if (response.Content.Headers.ContentDisposition != null)
            {
                if (!string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileNameStar))
                {
                    fileName = response.Content.Headers.ContentDisposition.FileNameStar;
                }
                else if (!string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
                {
                    fileName = response.Content.Headers.ContentDisposition.FileName;
                }
            }

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"{id}.bin";
            }

            if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
            {
                fileName = fileName.Substring(1, fileName.Length - 2);
            }

            string savePath = Path.Combine(saveToDir, fileName);

            await using Stream responseStream = await response.Content.ReadAsStreamAsync();
            await using FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write);

            byte[] buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await responseStream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
            {
                await fs.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;
                progressCallback?.Invoke(totalRead);
            }
        }

        public static async Task<Models.FileInfoResponse?> GetFileInfoAsync(string id, string password, string serverUrl)
        {
            string url = $"{serverUrl}/info?id={WebUtility.UrlEncode(id)}&pass={WebUtility.UrlEncode(password)}";

            using HttpClient httpClient = new HttpClient();
            using HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Models.FileInfoResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private class StreamWithProgressContent : HttpContent
        {
            private readonly HttpContent _originalContent;
            private readonly Action<long>? _progress;
            private int _bufferSize = MinBufferSize;

            public StreamWithProgressContent(HttpContent content, Action<long>? progress)
            {
                _originalContent = content;
                _progress = progress;

                foreach (KeyValuePair<string, IEnumerable<string>> header in _originalContent.Headers)
                {
                    Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            {
                long total = 0;
                byte[] buffer = new byte[_bufferSize];
                using Stream inputStream = await _originalContent.ReadAsStreamAsync();

                while (true)
                {
                    int read = await inputStream.ReadAsync(buffer, 0, buffer.Length);
                    if (read <= 0)
                        break;

                    await stream.WriteAsync(buffer, 0, read);
                    total += read;
                    _progress?.Invoke(total);

                    if (_bufferSize < MaxBufferSize)
                    {
                        _bufferSize = Math.Min(_bufferSize * 2, MaxBufferSize);
                    }
                }
            }

            protected override bool TryComputeLength(out long length)
            {
                if (_originalContent.Headers.ContentLength.HasValue)
                {
                    length = _originalContent.Headers.ContentLength.Value;
                    return true;
                }

                length = -1;
                return false;
            }
        }
    }
}
