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
        public static async Task<string?> UploadSecureAsync(string filePath, string serverUrl, Action<long, double>? progressCallback = null)
        {
            using HttpClient httpClient = new HttpClient();
            using MultipartFormDataContent form = new MultipartFormDataContent();
            using FileStream fileStream = File.OpenRead(filePath);
            using StreamContent rawContent = new StreamContent(fileStream);
            using ContentStreamer uploadContent = new ContentStreamer(rawContent, progressCallback);
            string? responseContent = null;

            uploadContent.Headers.ContentType = new MediaTypeHeaderValue(Constants.HEADER_APPLICATION_STREAM);
            form.Add(uploadContent, "file", Path.GetFileName(filePath));

            HttpResponseMessage response;

            try
            {
                response = await httpClient.PostAsync($"{serverUrl}/upload", form);
            }
            catch (HttpRequestException ex)
            {
                return null;
            }

            responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return responseContent;
        }

        private static string GetFileNameFromContentDisposition(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentDisposition != null)
            {
                if (!string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileNameStar))
                {
                    return response.Content.Headers.ContentDisposition.FileNameStar;
                }
                else if (!string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
                {
                    return response.Content.Headers.ContentDisposition.FileName;
                }
            }

            return string.Empty;
        }

        public static async Task DownloadSecureAsync(string id, string password, string serverUrl, string saveToDir, Action<long, double>? progressCallback = null)
        {
            int bytesRead = 0;
            long totalRead = 0;
            string? fileName = null;
            string? savePath = null;
            byte[] buffer = new byte[8192];
            string url = $"{serverUrl}/download?id={WebUtility.UrlEncode(id)}&pass={WebUtility.UrlEncode(password)}";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            long lastBytes = 0;
            double speed = 0;

            using HttpClient httpClient = new HttpClient();
            using HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            fileName = CleanFileName(GetFileNameFromContentDisposition(response), id);
            savePath = Path.Combine(saveToDir, fileName);

            await using Stream responseStream = await response.Content.ReadAsStreamAsync();
            await using FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write);

            while ((bytesRead = await responseStream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
            {
                await fs.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;

                if (stopwatch.ElapsedMilliseconds >= 1000)
                {
                    long bytesSinceLast = totalRead - lastBytes;
                    speed = bytesSinceLast / (stopwatch.ElapsedMilliseconds / 1000.0);

                    stopwatch.Restart();
                    lastBytes = totalRead;
                }

                progressCallback?.Invoke(totalRead, speed);
            }

        }

        private static string CleanFileName(string fileName, string id)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"{id}.bin";
            }

            if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
            {
                fileName = fileName.Substring(1, fileName.Length - 2);
            }

            return fileName;
        }

        public static async Task<Models.FileInfoResponse?> GetFileInfoAsync(string id, string password, string serverUrl)
        {
            string url = $"{serverUrl}/info?id={WebUtility.UrlEncode(id)}&pass={WebUtility.UrlEncode(password)}";
            string? json = null;
            using HttpClient httpClient = new HttpClient();
            using HttpResponseMessage response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<Models.FileInfoResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;
        }
    }
}
