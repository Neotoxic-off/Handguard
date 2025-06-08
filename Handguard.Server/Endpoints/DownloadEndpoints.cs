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
            app.MapGet("/download", async (HttpContext context, FileStorageService storage, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("DownloadEndpoints");
                HttpRequest request = context.Request;
                HttpResponse response = context.Response;

                (string id, string password) = ExtractDownloadParameters(request);

                if (!IsValidDownloadRequest(id, password))
                {
                    logger.LogWarning(Messages.Log_DownloadInvalidRequest, id);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    await response.WriteAsync(Messages.Download_InvalidRequest);
                    return;
                }

                FileDownloadResult? result = await storage.GetFileAsync(id, password);

                if (result == null)
                {
                    logger.LogWarning(Messages.Log_DownloadFailed, id);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    await response.WriteAsync(Messages.Download_NotFound);
                    return;
                }

                logger.LogInformation(Messages.Log_DownloadSuccess, id);

                response.ContentType = result.ContentType;
                response.ContentLength = result.Length;
                response.Headers.ContentDisposition = $"attachment; filename=\"{result.FileName}\"";

                await result.Stream.CopyToAsync(response.Body);
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
