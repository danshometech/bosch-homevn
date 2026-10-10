using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.Catalog;
using BoschHomeVn.Application.Catalog.GetMenu;
using BoschHomeVn.Application.Catalog.GetProductArticle;
using BoschHomeVn.Application.Catalog.GetProductById;
using BoschHomeVn.Application.Catalog.GetProductListing;
using BoschHomeVn.Application.Catalog.GetProducts;
using BoschHomeVn.Contracts.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class CatalogController : ControllerBase
{
    private const int MaxIds = 50;
    private const int MaxTake = 100;
    private const int DefaultPageSize = 9;
    private const int MaxPageSize = 48;

    [HttpGet("menu")]
    public async Task<MenuResponse> GetMenu([FromServices] GetMenuHandler handler, CancellationToken cancellationToken)
    {
        var menu = await handler.Handle(cancellationToken);
        return new MenuResponse([.. menu.Categories.Select(c => c.ToResponse())], menu.FlashSaleCount);
    }

    [HttpGet("products")]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetProducts(
        [FromServices] GetProductsHandler handler,
        [FromQuery] string? category,
        [FromQuery] string? q,
        [FromQuery] bool flash,
        [FromQuery] string? ids,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        string[]? idList = ids?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (idList?.Length > MaxIds)
        {
            return Problem($"Tối đa {MaxIds} mã mỗi lần.", statusCode: StatusCodes.Status400BadRequest);
        }
        if (take is < 1 or > MaxTake)
        {
            return Problem($"take phải trong khoảng 1–{MaxTake}.", statusCode: StatusCodes.Status400BadRequest);
        }

        var products = await handler.Handle(new ProductFilter(category, q, flash, idList, take), cancellationToken);
        return products.Select(p => p.ToResponse()).ToList();
    }

    [HttpGet("products/listing")]
    public async Task<ActionResult<ProductListingResponse>> GetListing(
        [FromServices] GetProductListingHandler handler,
        [FromQuery] string? category,
        [FromQuery] string? q,
        [FromQuery] bool flash,
        [FromQuery(Name = "type")] string[]? types,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] decimal? minRating,
        [FromQuery] string? sort,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize)
    {
        if (page < 1 || pageSize is < 1 or > MaxPageSize)
        {
            return Problem($"page ≥ 1, pageSize trong khoảng 1–{MaxPageSize}.", statusCode: StatusCodes.Status400BadRequest);
        }
        if (types?.Length > MaxIds)
        {
            return Problem($"Tối đa {MaxIds} loại mỗi lần.", statusCode: StatusCodes.Status400BadRequest);
        }
        var order = sort switch
        {
            null or "" or "pop" => ProductSort.Popular,
            "off" => ProductSort.Discount,
            "asc" => ProductSort.PriceAsc,
            "desc" => ProductSort.PriceDesc,
            _ => (ProductSort?)null,
        };
        if (order is null)
        {
            return Problem("sort phải là pop, off, asc hoặc desc.", statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await handler.Handle(
            new ProductListingQuery(category, q, flash, types, minPrice, maxPrice, minRating, order.Value, page, pageSize),
            cancellationToken);
        return new ProductListingResponse(
            [.. result.Items.Select(p => p.ToResponse())],
            result.Page,
            pageSize,
            result.Total,
            [.. result.TypeCounts.Select(t => new ProductTypeCountResponse(t.TypeName, t.Count))]);
    }

    [HttpGet("products/{id}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(
        string id, [FromServices] GetProductByIdHandler handler, CancellationToken cancellationToken)
    {
        var product = await handler.Handle(id, cancellationToken);
        return product is null ? NotFound() : product.ToResponse();
    }

    [HttpGet("products/{id}/article")]
    public async Task<ActionResult<ProductArticleResponse>> GetArticle(
        string id, [FromServices] GetProductArticleHandler handler, CancellationToken cancellationToken)
    {
        var html = await handler.Handle(id, cancellationToken);
        return html is null ? NotFound() : new ProductArticleResponse(html);
    }
}
