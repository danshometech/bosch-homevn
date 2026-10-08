using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BoschHomeVn.Infrastructure.Identity;

public static class AdminAccountSeeder
{
    public const string AdminRole = "Admin";

    public static async Task EnsureAdminAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        using var scope = services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roles.RoleExistsAsync(AdminRole))
        {
            await roles.CreateAsync(new IdentityRole(AdminRole));
        }
        if ((await users.GetUsersInRoleAsync(AdminRole)).Count > 0)
        {
            return;
        }

        var userName = configuration["Admin:UserName"];
        var email = configuration["Admin:Email"];
        var password = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Chưa có tài khoản admin — đặt Admin:UserName, Admin:Email và Admin:Password để tạo khi khởi động.");
            return;
        }

        var user = new IdentityUser { UserName = userName, Email = email, EmailConfirmed = true };
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            throw new InvalidOperationException(
                "Không tạo được tài khoản admin: " + string.Join(" ", created.Errors.Select(e => e.Description)));
        }

        await users.AddToRoleAsync(user, AdminRole);
        logger.LogInformation("Đã tạo tài khoản admin {UserName}", userName);
    }
}
