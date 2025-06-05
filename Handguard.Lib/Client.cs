using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Handguard.Lib
{
    public static class Client
    {
        private const int MinBufferSize = 8192;
        private const int MaxBufferSize = 262144;

        public static async Task<Dictionary<string, string>?> UploadSecureAsync(string zipFilePath, string serverUrl, Action<long>? progressCallback = null)
        {
            string? json = null;

            using HttpClient httpClient = new HttpClient();
            using MultipartFormDataContent form = new MultipartFormDataContent();
            using FileStream fileStream = File.OpenRead(zipFilePath);
            StreamContent streamContent = new StreamContent(fileStream);
            HttpResponseMessage? response = null;

            form.Add(streamContent, "file", Path.GetFileName(zipFilePath));
            response = await httpClient.PostAsync($"{serverUrl}/upload", form);
            response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        public static async Task DownloadSecureAsync(string id, string password, string serverUrl, string saveToPath)
        {
            FileStream? fs = null;
            using HttpClient httpClient = new HttpClient();
            string url = $"{serverUrl}/download?id={WebUtility.UrlEncode(id)}&pass={WebUtility.UrlEncode(password)}";
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                fs = new FileStream(saveToPath, FileMode.Create, FileAccess.Write);
                await response.Content.CopyToAsync(fs);
            }
        }

        private class StreamWithProgressContent : HttpContent
        {
            private readonly HttpContent _originalContent;
            private readonly Action<long>? _progress;
            private int _currentBufferSize = MinBufferSize;

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
                int bytesRead = 0;
                bool running = true;
                long totalUploaded = 0;
                byte[] buffer = new byte[0];
                using Stream inputStream = await _originalContent.ReadAsStreamAsync();

                while (running == true)
                {
                    buffer = new byte[_currentBufferSize];
                    bytesRead = await inputStream.ReadAsync(buffer, 0, _currentBufferSize);
                    if (bytesRead > 0)
                    {
                        await stream.WriteAsync(buffer, 0, bytesRead);
                        totalUploaded += bytesRead;
                        _progress?.Invoke(totalUploaded);

                        if (_currentBufferSize < MaxBufferSize)
                            _currentBufferSize = Math.Min(_currentBufferSize * 2, MaxBufferSize);
                    }
                    else
                    {
                        running = false;
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
