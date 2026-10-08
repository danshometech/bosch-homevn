using BoschHomeVn.Api;
using BoschHomeVn.Application;
using BoschHomeVn.Infrastructure;
using BoschHomeVn.Infrastructure.Identity;
using BoschHomeVn.Infrastructure.Media;
using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

const string mediaRequestPath = "/media";
var mediaRoot = Path.GetFullPath(builder.Configuration["Media:RootPath"] ?? "App_Data/media", builder.Environment.ContentRootPath);
Directory.CreateDirectory(mediaRoot);
var seededMedia = MediaSeeder.CopyMissing(Path.Combine(builder.Environment.ContentRootPath, "SeedMedia"), mediaRoot);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, mediaRoot, mediaRequestPath)
    .AddPresentation(builder.Configuration, builder.Environment);

var app = builder.Build();
if (seededMedia > 0)
{
    app.Logger.LogInformation("Đã chép {Count} ảnh từ SeedMedia sang {MediaRoot}", seededMedia, mediaRoot);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // Tài liệu API: /openapi/v1.json và giao diện /scalar
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(mediaRoot), RequestPath = mediaRequestPath });
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await app.Services.MigrateDatabaseAsync();
}

try
{
    await AdminAccountSeeder.EnsureAdminAsync(app.Services, app.Configuration, app.Logger);
}
catch (Exception ex) when (ex is not InvalidOperationException)
{
    app.Logger.LogError(ex, "Không kiểm tra được tài khoản admin — DB đã tạo chưa?");
}

app.Run();
