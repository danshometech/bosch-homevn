using BoschHomeVn.Application.Checkout;
using BoschHomeVn.Contracts.Checkout;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers;

[ApiController]
[Route("api/checkout")]
public sealed class CheckoutController(CheckoutOptionsHandler handler) : ControllerBase
{
    [HttpGet("options")]
    public async Task<CheckoutOptionsResponse> GetOptions(CancellationToken cancellationToken)
    {
        var options = await handler.GetAsync(cancellationToken);
        return new CheckoutOptionsResponse(
            [.. options.Shipping.Select(m => new ShippingMethodResponse(m.Id, m.Name, m.Description, m.Fee))],
            [.. options.Payment.Select(m => new PaymentMethodResponse(m.Id, m.Name, m.Description, m.IsInstallment))]);
    }
}
