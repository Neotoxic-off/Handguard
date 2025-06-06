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

        public static async Task<Dictionary<string, string>?> UploadSecureAsync(string filePath, string serverUrl, Action<long>? progressCallback = null)
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

            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        public static async Task DownloadSecureAsync(string id, string password, string serverUrl, string saveToDir)
        {
            using HttpClient httpClient = new HttpClient();
            string url = $"{serverUrl}/download?id={WebUtility.UrlEncode(id)}&pass={WebUtility.UrlEncode(password)}";

            using HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Download failed with status code {response.StatusCode}");

            string? fileName = null;
            if (response.Content.Headers.ContentDisposition?.FileNameStar != null)
                fileName = response.Content.Headers.ContentDisposition.FileNameStar;
            else if (response.Content.Headers.ContentDisposition?.FileName != null)
                fileName = response.Content.Headers.ContentDisposition.FileName;

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = $"{id}.bin";

            string savePath = Path.Combine(saveToDir, fileName);

            await using Stream responseStream = await response.Content.ReadAsStreamAsync();
            await using FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write);
            await responseStream.CopyToAsync(fs);
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
                    Headers.TryAddWithoutValidation(header.Key, header.Value);
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
                        _bufferSize = Math.Min(_bufferSize * 2, MaxBufferSize);
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
