using BoschHomeVn.Application.Abstractions.Persistence;

namespace BoschHomeVn.Application.Catalog.GetProductListing;

// Trang danh sách sản phẩm: lọc, sắp xếp, phân trang ở DB
public sealed class GetProductListingHandler(IAppDbContext db)
{
    public Task<ProductListingPage> Handle(ProductListingQuery query, CancellationToken cancellationToken) =>
        db.ProductListingAsync(query, cancellationToken);
}
