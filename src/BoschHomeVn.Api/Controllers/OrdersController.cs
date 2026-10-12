using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.Orders;
using BoschHomeVn.Contracts.Orders;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(PlaceOrderHandler handler) : ControllerBase
{
    [HttpPost]
    public async Task<PlaceOrderResponse> Place(PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await handler.HandleAsync(request.ToCommand(), cancellationToken);
        return new PlaceOrderResponse(order.Code, order.Total);
    }
}
