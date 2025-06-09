using Handguard.Server.Services;
using Handguard.Server.Endpoints;
using Microsoft.AspNetCore.Http.Features;
using Handguard.Server;
using Handguard.Server.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = Settings.MaxUploadSize;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = Settings.MaxUploadSize;
});

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddHostedService<CleanupService>();
builder.Services.AddSingleton<FileStorageService>(sp =>
{
    string storagePath = Path.Combine(AppContext.BaseDirectory, "storage");
    return new FileStorageService(storagePath);
});

WebApplication app = builder.Build();

app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();

    if (context.Request.ContentLength is long length && length > Settings.MaxUploadSize)
    {
        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
        await context.Response.WriteAsync($"Upload failed: File size exceeds {Settings.MaxUploadSize} GB limit.");

        return;
    }

    await next();
});

app.MapGet("/", () => Results.Ok("Handguard server is running"));

app.MapUploadEndpoints();
app.MapDownloadEndpoints();
app.MapInformationEndpoints();

app.Run();
