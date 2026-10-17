using System.Text.RegularExpressions;
using BoschHomeVn.Contracts.Reviews;
using BoschHomeVn.WebStore.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Controllers;

[ApiController]
[Route("api")]
public sealed partial class ReviewsController(StoreApiClient api, TurnstileVerifier captcha, IWebHostEnvironment env) : ControllerBase
{
    private const string CaptchaHeader = "X-Captcha-Token";
    private const long MaxRequestBytes = 25 * 1024 * 1024 + 64 * 1024;
    // Mã ngẫu nhiên cho mỗi trình duyệt để mỗi trình duyệt chỉ bấm "Hữu ích" một đánh giá một lần (không chứa thông tin cá nhân)
    private const string VisitorCookie = "bh_vid";

    [HttpGet("products/{id}/reviews")]
    public Task<ActionResult<ReviewPageResponse>> List(string id, CancellationToken cancellationToken) =>
        api.GetReviewsAsync(id, Request.QueryString, VisitorId(), cancellationToken);

    [HttpPost("products/{id}/reviews")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<ActionResult<SubmitReviewResponse>> Submit(string id, CancellationToken cancellationToken)
    {
        if (!Request.HasFormContentType)
        {
            return Problem(statusCode: StatusCodes.Status415UnsupportedMediaType, title: "Gửi đánh giá dạng multipart/form-data.");
        }
        if (captcha.Enabled && !await captcha.VerifyAsync(Request.Headers[CaptchaHeader], cancellationToken))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Xác minh chống spam chưa thành công, vui lòng bấm Gửi đánh giá lần nữa.");
        }

        var form = await Request.ReadFormAsync(cancellationToken);
        return await api.SubmitReviewAsync(id, form, cancellationToken);
    }

    [HttpPost("reviews/{id:guid}/helpful")]
    public Task<ActionResult<HelpfulResponse>> Helpful(Guid id, CancellationToken cancellationToken) =>
        api.HelpfulAsync(id, true, EnsureVisitorId(), cancellationToken);

    [HttpDelete("reviews/{id:guid}/helpful")]
    public Task<ActionResult<HelpfulResponse>> Unhelpful(Guid id, CancellationToken cancellationToken) =>
        api.HelpfulAsync(id, false, EnsureVisitorId(), cancellationToken);

    private string? VisitorId() =>
        Request.Cookies.TryGetValue(VisitorCookie, out var value) && VisitorIdFormat().IsMatch(value) ? value : null;

    private string EnsureVisitorId()
    {
        if (VisitorId() is { } id)
        {
            return id;
        }

        id = Guid.NewGuid().ToString("N");
        Response.Cookies.Append(VisitorCookie, id, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            // Host nằm sau proxy HTTPS (request tới đây là http) nên phải ép Secure
            Secure = !env.IsDevelopment(),
            MaxAge = TimeSpan.FromDays(365),
            IsEssential = true,
        });
        return id;
    }

    [GeneratedRegex("^[a-f0-9]{32}$")]
    private static partial Regex VisitorIdFormat();
}
