using BoschHomeVn.Contracts.Checkout;
using BoschHomeVn.WebStore.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Controllers;

[ApiController]
[Route("api/checkout")]
public sealed class CheckoutController(StoreApiClient api) : ControllerBase
{
    [HttpGet("options")]
    public Task<ActionResult<CheckoutOptionsResponse>> GetOptions(CancellationToken cancellationToken) =>
        api.GetCheckoutOptionsAsync(cancellationToken);
}
