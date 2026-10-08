using BoschHomeVn.Api.Infrastructure;
using BoschHomeVn.Contracts.Admin;
using BoschHomeVn.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers.Admin;

// Đăng nhập trang quản trị bằng cookie HttpOnly (ASP.NET Core Identity). Sai 5 lần thì khóa 15 phút.
[ApiController]
[Route("api/admin/auth")]
public sealed class AuthController(SignInManager<IdentityUser> signIn, UserManager<IdentityUser> users) : ControllerBase
{
    private const string WrongCredentials = "Tên đăng nhập hoặc mật khẩu không đúng.";

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AdminUserResponse>> Login(LoginRequest request)
    {
        var user = await users.FindByNameAsync(request.UserName) ?? await users.FindByEmailAsync(request.UserName);
        if (user is null || !await users.IsInRoleAsync(user, AdminAccountSeeder.AdminRole))
        {
            return Problem(WrongCredentials, statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await signIn.PasswordSignInAsync(user, request.Password, request.Remember, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            return Problem("Tài khoản tạm khóa do nhập sai nhiều lần, thử lại sau 15 phút.", statusCode: StatusCodes.Status401Unauthorized);
        }
        if (!result.Succeeded)
        {
            return Problem(WrongCredentials, statusCode: StatusCodes.Status401Unauthorized);
        }

        return new AdminUserResponse(user.UserName!);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return NoContent();
    }

    // Trang quản trị gọi khi mở để biết còn phiên đăng nhập không (401 = chưa đăng nhập)
    [HttpGet("me")]
    [Authorize(Policy = AdminAuth.Policy)]
    public ActionResult<AdminUserResponse> Me() => new AdminUserResponse(User.Identity!.Name!);
}
