namespace BoschHomeVn.Api.Infrastructure;

internal static class AdminAuth
{
    // [Authorize(Policy = AdminAuth.Policy)] cho mọi controller /api/admin/* (trừ đăng nhập)
    public const string Policy = "Admin";
}
