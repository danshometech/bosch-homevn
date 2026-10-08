using BoschHomeVn.Admin.Infrastructure;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

// Đăng nhập do Api xử lý (Identity); cookie bh_admin đi qua ApiCookieRelayHandler
[ApiController]
[Route("api/admin/auth")]
public sealed class AuthController(AdminApiClient api) : ControllerBase
{
    [HttpPost("login")]
    public Task<ActionResult<AdminUserResponse>> Login(LoginRequest request, CancellationToken cancellationToken) =>
        api.PostAsync<AdminUserResponse>("api/admin/auth/login", request, cancellationToken);

    [HttpPost("logout")]
    public Task<IActionResult> Logout(CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Post, "api/admin/auth/logout", null, cancellationToken);

    // Chưa đăng nhập: Api trả 401, Vue chuyển sang trang đăng nhập
    [HttpGet("me")]
    public Task<ActionResult<AdminUserResponse>> Me(CancellationToken cancellationToken) =>
        api.GetAsync<AdminUserResponse>("api/admin/auth/me", cancellationToken);
}
