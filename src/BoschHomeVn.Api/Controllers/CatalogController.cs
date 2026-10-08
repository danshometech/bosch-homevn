using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.Catalog;
using BoschHomeVn.Application.Catalog.GetMenu;
using BoschHomeVn.Application.Catalog.GetProductById;
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

    [HttpGet("products/{id}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(
        string id, [FromServices] GetProductByIdHandler handler, CancellationToken cancellationToken)
    {
        var product = await handler.Handle(id, cancellationToken);
        return product is null ? NotFound() : product.ToResponse();
    }
}
