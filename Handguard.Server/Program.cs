using Handguard.Server.Services;
using Handguard.Server.Endpoints;
using Microsoft.AspNetCore.Http.Features;
using Handguard.Server;
using Handguard.Server.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 10L * 1024 * 1024 * 1024;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10L * 1024 * 1024 * 1024;
});

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddHostedService<CleanupService>();
builder.Services.AddSingleton<FileStorageService>(sp =>
{
    var storagePath = Path.Combine(AppContext.BaseDirectory, "storage");
    return new FileStorageService(storagePath);
});

WebApplication app = builder.Build();

app.MapGet("/", () => Results.Ok("Handguard server is running"));

app.MapUploadEndpoints();
app.MapDownloadEndpoints();
app.MapInformationEndpoints();

app.Run();
