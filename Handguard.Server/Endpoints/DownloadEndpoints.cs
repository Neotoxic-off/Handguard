using Handguard.Server.Common;
using Handguard.Server.Models;
using Handguard.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Handguard.Server.Endpoints
{
    public static class DownloadEndpoints
    {
        public static void MapDownloadEndpoints(this WebApplication app)
        {
            app.MapGet("/download", async (HttpRequest request, FileStorageService storage, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("DownloadEndpoints");

                (string id, string password) = ExtractDownloadParameters(request);

                if (!IsValidDownloadRequest(id, password))
                {
                    logger.LogWarning(Messages.Log_DownloadInvalidRequest, id);
                    return Results.BadRequest(Messages.Download_InvalidRequest);
                }

                FileDownloadResult? result = await storage.GetFileAsync(id, password);

                if (result == null)
                {
                    logger.LogWarning(Messages.Log_DownloadFailed, id);
                    return Results.BadRequest(Messages.Download_NotFound);
                }

                logger.LogInformation(Messages.Log_DownloadSuccess, id);

                return Results.File(
                    result.Stream,
                    result.ContentType,
                    result.FileName,
                    enableRangeProcessing: true
                );
            });
        }

        private static (string id, string password) ExtractDownloadParameters(HttpRequest request)
        {
            string id = request.Query["id"].ToString();
            string password = request.Query["pass"].ToString();
            return (id, password);
        }

        private static bool IsValidDownloadRequest(string id, string password)
        {
            return !string.IsNullOrEmpty(id) &&
                   !string.IsNullOrEmpty(password) &&
                   SecurityService.IsValidId(id);
        }
    }
}
