namespace BoschHomeVn.Admin.Infrastructure;

// Phiên đăng nhập do Api cấp (cookie bh_admin, HttpOnly): gửi kèm cookie của trình duyệt khi gọi Api; Api cấp mới /
// gia hạn / xóa cookie (Set-Cookie: đăng nhập, đăng xuất, gia hạn trượt) thì chuyển nguyên về trình duyệt
public sealed class ApiCookieRelayHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    // Cookie lớn bị ASP.NET chia thành bh_admin, bh_adminC1, bh_adminC2…
    private const string CookiePrefix = "bh_admin";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = accessor.HttpContext;
        var cookies = context?.Request.Headers.Cookie
            .SelectMany(h => (h ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(c => c.StartsWith(CookiePrefix, StringComparison.Ordinal))
            .ToList();
        if (cookies is { Count: > 0 })
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies));
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (context is { Response.HasStarted: false } && response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            foreach (var value in setCookies)
            {
                context.Response.Headers.Append("Set-Cookie", value);
            }
        }
        return response;
    }
}
