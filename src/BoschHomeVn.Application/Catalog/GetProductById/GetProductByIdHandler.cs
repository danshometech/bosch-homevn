using BoschHomeVn.Application.Abstractions.Persistence;

namespace BoschHomeVn.Application.Catalog.GetProductById;

// Một sản phẩm đang bán; null nếu không có hoặc chưa có giá bán
public sealed class GetProductByIdHandler(IAppDbContext db)
{
    public async Task<ProductListItem?> Handle(string id, CancellationToken cancellationToken)
    {
        var items = await db.SellableProductsAsync(new ProductFilter(Ids: [id]), cancellationToken);
        return items.Count > 0 ? items[0] : null;
    }
}
