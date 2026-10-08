using BoschHomeVn.Application.Abstractions.Persistence;

namespace BoschHomeVn.Application.Catalog.GetProducts;

// Danh sách sản phẩm đang bán theo bộ lọc (trang danh sách, tìm kiếm, flash sale, giỏ hàng theo mã)
public sealed class GetProductsHandler(IAppDbContext db)
{
    public Task<IReadOnlyList<ProductListItem>> Handle(ProductFilter filter, CancellationToken cancellationToken) =>
        db.SellableProductsAsync(filter, cancellationToken);
}
