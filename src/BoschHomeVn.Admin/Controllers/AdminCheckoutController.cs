using BoschHomeVn.Admin.Infrastructure;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

[ApiController]
[Route("api/admin/checkout")]
public sealed class AdminCheckoutController(AdminApiClient api) : ControllerBase
{
    private const string Base = "api/admin/checkout/";

    [HttpGet("shipping")]
    public Task<ActionResult<IReadOnlyList<AdminShippingMethodResponse>>> ListShipping(CancellationToken cancellationToken) =>
        api.GetAsync<IReadOnlyList<AdminShippingMethodResponse>>(Base + "shipping", cancellationToken);

    [HttpPost("shipping")]
    public Task<ActionResult<CreatedResponse>> CreateShipping(SaveShippingMethodRequest request, CancellationToken cancellationToken) =>
        api.PostAsync<CreatedResponse>(Base + "shipping", request, cancellationToken);

    [HttpPut("shipping/order")]
    public Task<IActionResult> ReorderShipping(ReorderRequest request, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Put, Base + "shipping/order", request, cancellationToken);

    [HttpPut("shipping/{id}")]
    public Task<IActionResult> UpdateShipping(string id, SaveShippingMethodRequest request, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Put, Base + "shipping/" + Uri.EscapeDataString(id), request, cancellationToken);

    [HttpDelete("shipping/{id}")]
    public Task<IActionResult> DeleteShipping(string id, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Delete, Base + "shipping/" + Uri.EscapeDataString(id), null, cancellationToken);

    [HttpGet("payment")]
    public Task<ActionResult<IReadOnlyList<AdminPaymentMethodResponse>>> ListPayment(CancellationToken cancellationToken) =>
        api.GetAsync<IReadOnlyList<AdminPaymentMethodResponse>>(Base + "payment", cancellationToken);

    [HttpPost("payment")]
    public Task<ActionResult<CreatedResponse>> CreatePayment(SavePaymentMethodRequest request, CancellationToken cancellationToken) =>
        api.PostAsync<CreatedResponse>(Base + "payment", request, cancellationToken);

    [HttpPut("payment/order")]
    public Task<IActionResult> ReorderPayment(ReorderRequest request, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Put, Base + "payment/order", request, cancellationToken);

    [HttpPut("payment/{id}")]
    public Task<IActionResult> UpdatePayment(string id, SavePaymentMethodRequest request, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Put, Base + "payment/" + Uri.EscapeDataString(id), request, cancellationToken);

    [HttpDelete("payment/{id}")]
    public Task<IActionResult> DeletePayment(string id, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Delete, Base + "payment/" + Uri.EscapeDataString(id), null, cancellationToken);
}
