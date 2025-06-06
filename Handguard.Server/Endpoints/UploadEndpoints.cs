using Handguard.Server.Common;
using Handguard.Server.Models;
using Handguard.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Handguard.Server.Endpoints
{
    public static class UploadEndpoints
    {
        public static void MapUploadEndpoints(this WebApplication app)
        {
            app.MapPost("/upload", async (HttpRequest request, FileStorageService storage, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("UploadEndpoints");

                if (!IsValidUploadRequest(request))
                {
                    logger.LogWarning(Messages.Log_UploadNoFile);
                    return Results.BadRequest(Messages.Upload_NoFile);
                }

                IFormFile file = request.Form.Files[0];

                if (!IsValidFile(file))
                {
                    logger.LogWarning(Messages.Log_UploadInvalidFile, file?.FileName);
                    return Results.BadRequest(Messages.Upload_InvalidFile);
                }

                UploadResponse response = await storage.SaveFileAsync(file);
                logger.LogInformation(Messages.Log_UploadSuccess, response.Id);
                logger.LogInformation($"{response.Id} {response.Password}");

                return Results.Ok(response);
            });
        }

        private static bool IsValidUploadRequest(HttpRequest request)
        {
            return request.HasFormContentType && request.Form.Files.Count > 0;
        }

        private static bool IsValidFile(IFormFile file)
        {
            return file.Length > 0 && !string.IsNullOrWhiteSpace(file.FileName);
        }
    }
}
