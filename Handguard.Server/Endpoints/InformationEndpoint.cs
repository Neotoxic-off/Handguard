using Handguard.Server.Common;
using Handguard.Server.Models;
using Handguard.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Handguard.Server.Endpoints
{
    public static class InformationEndpoint
    {
        public static void MapInformationEndpoints(this WebApplication app)
        {
            app.MapGet("/info", async (HttpRequest request, FileStorageService storage, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("InfoEndpoint");

                // Extract parameters from query
                string id = request.Query["id"].ToString();
                string password = request.Query["pass"].ToString();

                // Basic validation
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password) || !SecurityService.IsValidId(id))
                {
                    logger.LogWarning(Messages.Log_DownloadInvalidRequest, id);
                    return Results.BadRequest(Messages.Download_InvalidRequest);
                }

                // Fetch file metadata
                FileDownloadResult? result = await storage.GetFileAsync(id, password);
                if (result == null)
                {
                    logger.LogWarning(Messages.Log_DownloadFailed, id);
                    return Results.NotFound(Messages.Download_NotFound);
                }

                logger.LogInformation("Info fetch successful for ID {Id}", id);

                return Results.Json(new
                {
                    FileName = result.FileName,
                    ContentType = result.ContentType,
                    Size = result.Stream.Length
                });
            });
        }
    }
}
