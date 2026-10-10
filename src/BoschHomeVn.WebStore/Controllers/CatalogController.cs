using BoschHomeVn.Contracts.Catalog;
using BoschHomeVn.WebStore.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Controllers;

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

    [HttpGet("products/listing")]
    public Task<ActionResult<ProductListingResponse>> GetListing(CancellationToken cancellationToken) =>
        api.GetProductListingAsync(Request.QueryString, cancellationToken);

    [HttpGet("products/{id}")]
    public Task<ActionResult<ProductResponse>> GetProduct(string id, CancellationToken cancellationToken) =>
        api.GetProductAsync(id, cancellationToken);

    [HttpGet("products/{id}/article")]
    public Task<ActionResult<ProductArticleResponse>> GetArticle(string id, CancellationToken cancellationToken) =>
        api.GetProductArticleAsync(id, cancellationToken);
}
