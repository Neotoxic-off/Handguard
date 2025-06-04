using Handguard.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc; // For Results.File()
using System.IO;
using System.Threading.Tasks;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Set max upload size to 20 GB
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 20L * 1024 * 1024 * 1024; // 20 GB
});

// Register cleanup background service
builder.Services.AddHostedService<CleanupService>();

WebApplication app = builder.Build();

app.MapGet("/", () => Results.Ok("Handguard server is running"));

app.MapPost("/upload", async (HttpRequest request) =>
{
    if (!request.HasFormContentType || request.Form.Files.Count == 0)
        return Results.BadRequest("No file uploaded.");

    IFormFile file = request.Form.Files[0];
    Microsoft.Extensions.Primitives.StringValues passValues = request.Form["pass"];
    string password = passValues.ToString();

    if (string.IsNullOrEmpty(password))
        return Results.BadRequest("Password is required.");

    string id = await FileStorage.SaveAsync(file, password);
    return Results.Ok(new { id = id });
});

app.MapGet("/download", async (HttpRequest request) =>
{
    Microsoft.Extensions.Primitives.StringValues idValues = request.Query["id"];
    Microsoft.Extensions.Primitives.StringValues passValues = request.Query["pass"];

    string id = idValues.ToString();
    string password = passValues.ToString();

    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        return Results.BadRequest("Id and password are required.");

    (Stream Stream, string FileName, string ContentType)? result = FileStorage.Get(id, password);

    if (!result.HasValue)
        return Results.NotFound("File not found or wrong password.");

    Stream stream = result.Value.Stream;
    string fileName = result.Value.FileName;
    string contentType = result.Value.ContentType;

    return Results.File(stream, contentType, fileName, enableRangeProcessing: true);
});

app.Run();
