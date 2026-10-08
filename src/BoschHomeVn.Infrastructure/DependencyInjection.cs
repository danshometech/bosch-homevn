using BoschHomeVn.Application.Abstractions.Media;
using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Infrastructure.Media;
using BoschHomeVn.Infrastructure.Persistence;
using BoschHomeVn.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoschHomeVn.Infrastructure;

public static class DependencyInjection
{
    // mediaRoot: thư mục lưu ảnh admin tải lên (đường dẫn tuyệt đối), mediaRequestPath: đường dẫn công khai (/media)
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration, string mediaRoot, string mediaRequestPath)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Thiếu cấu hình ConnectionStrings:Default.");
        }

        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString)
            // Seed khi bảng còn trống (chạy trong `dotnet ef database update` / Database.Migrate()):
            // danh mục + sản phẩm trước, rồi khoảnh khắc trang chủ (trỏ tới sản phẩm)
            .UseSeeding((context, _) =>
            {
                CatalogSeeder.Seed(context);
                HomeSeeder.Seed(context);
            })
            .UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                await CatalogSeeder.SeedAsync(context, cancellationToken);
                await HomeSeeder.SeedAsync(context, cancellationToken);
            }));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // Tài khoản trang quản trị (bảng AspNetUsers…); cookie đăng nhập cấu hình ở Api
        services
            .AddIdentityCore<IdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                // Tối thiểu 8 ký tự, có chữ thường + số + ký tự đặc biệt (không bắt chữ hoa)
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager();

        services.AddSingleton<IMediaStorage>(new LocalMediaStorage(mediaRoot, mediaRequestPath));

        return services;
    }

    // Chạy migration rồi seed (UseAsyncSeeding) — dùng khi triển khai container
    public static async Task MigrateDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }
}
