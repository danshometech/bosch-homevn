using BoschHomeVn.Api.Infrastructure;
using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.Dashboard;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers.Admin;

// Số liệu trang Tổng quan
[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = AdminAuth.Policy)]
public sealed class AdminDashboardController(GetDashboardHandler handler) : ControllerBase
{
    [HttpGet]
    public async Task<DashboardResponse> Get(CancellationToken cancellationToken)
    {
        var d = await handler.Handle(cancellationToken);
        return new DashboardResponse(
            d.Total,
            d.OnSale,
            d.NoPrice,
            d.Hidden,
            d.LowStock,
            d.OutOfStock,
            [.. d.Categories.Select(c => new DashboardCategoryResponse(c.Id, c.Name, c.Total, c.OnSale))],
            [.. d.OutOfStockProducts.Select(p => p.ToAdminResponse())]);
    }
}
