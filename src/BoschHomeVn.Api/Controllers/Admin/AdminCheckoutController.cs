using BoschHomeVn.Api.Infrastructure;
using BoschHomeVn.Application.Checkout.Admin;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/checkout")]
[Authorize(Policy = AdminAuth.Policy)]
public sealed class AdminCheckoutController(CheckoutAdminHandler handler) : ControllerBase
{
    [HttpGet("shipping")]
    public async Task<IReadOnlyList<AdminShippingMethodResponse>> ListShipping(CancellationToken cancellationToken) =>
        [.. (await handler.ListShippingAsync(cancellationToken)).Select(m => new AdminShippingMethodResponse(m.Id, m.Name, m.Description, m.Fee, m.IsActive))];

    [HttpPost("shipping")]
    public async Task<CreatedResponse> CreateShipping(SaveShippingMethodRequest request, CancellationToken cancellationToken) =>
        new(await handler.CreateShippingAsync(ToCommand(request), cancellationToken));

    [HttpPut("shipping/order")]
    public async Task<IActionResult> ReorderShipping(ReorderRequest request, CancellationToken cancellationToken)
    {
        await handler.ReorderShippingAsync(request.Ids, cancellationToken);
        return NoContent();
    }

    [HttpPut("shipping/{id}")]
    public async Task<IActionResult> UpdateShipping(string id, SaveShippingMethodRequest request, CancellationToken cancellationToken) =>
        await handler.UpdateShippingAsync(id, ToCommand(request), cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("shipping/{id}")]
    public async Task<IActionResult> DeleteShipping(string id, CancellationToken cancellationToken) =>
        await handler.DeleteShippingAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpGet("payment")]
    public async Task<IReadOnlyList<AdminPaymentMethodResponse>> ListPayment(CancellationToken cancellationToken) =>
        [.. (await handler.ListPaymentAsync(cancellationToken)).Select(m => new AdminPaymentMethodResponse(m.Id, m.Name, m.Description, m.IsInstallment, m.IsActive))];

    [HttpPost("payment")]
    public async Task<CreatedResponse> CreatePayment(SavePaymentMethodRequest request, CancellationToken cancellationToken) =>
        new(await handler.CreatePaymentAsync(ToCommand(request), cancellationToken));

    [HttpPut("payment/order")]
    public async Task<IActionResult> ReorderPayment(ReorderRequest request, CancellationToken cancellationToken)
    {
        await handler.ReorderPaymentAsync(request.Ids, cancellationToken);
        return NoContent();
    }

    [HttpPut("payment/{id}")]
    public async Task<IActionResult> UpdatePayment(string id, SavePaymentMethodRequest request, CancellationToken cancellationToken) =>
        await handler.UpdatePaymentAsync(id, ToCommand(request), cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("payment/{id}")]
    public async Task<IActionResult> DeletePayment(string id, CancellationToken cancellationToken) =>
        await handler.DeletePaymentAsync(id, cancellationToken) ? NoContent() : NotFound();

    private static SaveShippingMethodCommand ToCommand(SaveShippingMethodRequest r) => new(r.Name, r.Description, r.Fee, r.IsActive);

    private static SavePaymentMethodCommand ToCommand(SavePaymentMethodRequest r) => new(r.Name, r.Description, r.IsInstallment, r.IsActive);
}
