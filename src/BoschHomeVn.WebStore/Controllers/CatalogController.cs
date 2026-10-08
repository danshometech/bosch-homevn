using BoschHomeVn.Contracts.Catalog;
using BoschHomeVn.WebStore.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Controllers;

// Vue gọi host này; dữ liệu lấy từ BoschHomeVn.Api qua StoreApiClient
[ApiController]
[Route("api")]
public sealed class CatalogController(StoreApiClient api) : ControllerBase
{
    [HttpGet("menu")]
    public Task<ActionResult<MenuResponse>> GetMenu(CancellationToken cancellationToken) => api.GetMenuAsync(cancellationToken);

    [HttpGet("products")]
    public Task<ActionResult<IReadOnlyList<ProductResponse>>> GetProducts(
        [FromQuery] string? category,
        [FromQuery] string? q,
        [FromQuery] bool flash,
        [FromQuery] string? ids,
        [FromQuery] int? take,
        CancellationToken cancellationToken) =>
        api.GetProductsAsync(category, q, flash, ids, take, cancellationToken);

    [HttpGet("products/{id}")]
    public Task<ActionResult<ProductResponse>> GetProduct(string id, CancellationToken cancellationToken) =>
        api.GetProductAsync(id, cancellationToken);
}
