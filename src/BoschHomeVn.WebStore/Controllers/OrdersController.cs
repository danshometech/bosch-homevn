using BoschHomeVn.Contracts.Orders;
using BoschHomeVn.WebStore.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(StoreApiClient api, TurnstileVerifier captcha) : ControllerBase
{
    // Token Turnstile của trình duyệt gửi kèm qua header này
    private const string CaptchaHeader = "X-Captcha-Token";

    // SiteKey cho widget Turnstile trên trang thanh toán; null = đang tắt chống spam
    [HttpGet("captcha")]
    public CaptchaResponse Captcha() => new(captcha.Enabled ? captcha.SiteKey : null);

    [HttpPost]
    public async Task<ActionResult<PlaceOrderResponse>> Place(PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        if (captcha.Enabled && !await captcha.VerifyAsync(Request.Headers[CaptchaHeader], cancellationToken))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Xác minh chống spam chưa thành công, vui lòng xác minh lại rồi bấm Đặt hàng.");
        }
        return await api.PlaceOrderAsync(request, cancellationToken);
    }

    public sealed record CaptchaResponse(string? SiteKey);
}
