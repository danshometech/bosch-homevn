using BoschHomeVn.Api.Infrastructure;
using BoschHomeVn.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace BoschHomeVn.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApi();
        services.AddHealthChecks();

        // WebStore/Admin (và vite dev server) gọi Api từ origin khác
        var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddDefaultPolicy(policy =>
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

        // Khóa mã hóa cookie phải nằm trên volume, nếu không mỗi lần khởi động lại container là mọi người bị đăng xuất
        var keysPath = Path.GetFullPath(configuration["DataProtection:KeysPath"] ?? "App_Data/keys", environment.ContentRootPath);
        services.AddDataProtection().SetApplicationName("BoschHomeVn").PersistKeysToFileSystem(new DirectoryInfo(keysPath));

        // Đăng nhập trang quản trị: cookie HttpOnly của ASP.NET Core Identity. Trang quản trị gọi Api qua proxy
        // cùng origin nên SameSite=Strict được; API trả 401/403 thay vì chuyển hướng sang trang đăng nhập.
        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "bh_admin";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            // Api nằm sau proxy HTTPS, request tới nó là http nên phải ép Secure
            options.Cookie.SecurePolicy = environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });
        services.AddAuthorizationBuilder()
            .AddPolicy(AdminAuth.Policy, policy => policy.RequireRole(AdminAccountSeeder.AdminRole));

        return services;
    }
}
