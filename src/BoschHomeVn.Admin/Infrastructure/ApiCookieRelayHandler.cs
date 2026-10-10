using System.Diagnostics;

namespace BoschHomeVn.Admin.Infrastructure;

[DebuggerNonUserCode]
public sealed class ApiCookieRelayHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
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
