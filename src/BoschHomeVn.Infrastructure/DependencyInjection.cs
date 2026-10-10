using BoschHomeVn.Application.Abstractions.Content;
using BoschHomeVn.Application.Abstractions.Media;
using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Infrastructure.Content;
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

        services
            .AddIdentityCore<IdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager();

        services.AddSingleton<IMediaStorage>(new LocalMediaStorage(mediaRoot, mediaRequestPath));
        services.AddSingleton<IArticleSanitizer, ArticleSanitizer>();

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }
}
