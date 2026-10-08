using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Catalog;

public sealed record ProductListItem(Product Product, string CategoryId, string TypeName);

// Ids: lấy đúng các mã này (giỏ hàng, yêu thích) · Search: tìm không dấu trong tên sản phẩm + tên loại
public sealed record ProductFilter(
    string? CategoryId = null,
    string? Search = null,
    bool FlashSaleOnly = false,
    IReadOnlyCollection<string>? Ids = null,
    int? Take = null);

internal static class ProductQueries
{
    // Sản phẩm đang bán trên site: đã có giá bán (Price null = chờ admin nhập giá) và đang bật hiển thị, sắp theo thứ tự menu
    public static async Task<IReadOnlyList<ProductListItem>> SellableProductsAsync(
        this IAppDbContext db, ProductFilter filter, CancellationToken cancellationToken)
    {
        var query =
            from p in db.Products.AsNoTracking()
            join t in db.ProductTypes on p.ProductTypeId equals t.Id
            join c in db.Categories on t.CategoryId equals c.Id
            where p.Price != null && p.IsPublished
            select new { p, t, c };

        if (filter.CategoryId is { } categoryId)
        {
            query = query.Where(x => x.t.CategoryId == categoryId);
        }
        if (filter.FlashSaleOnly)
        {
            query = query.Where(x => x.p.IsFlashSale);
        }
        if (filter.Ids is { } ids)
        {
            query = query.Where(x => ids.Contains(x.p.Id));
        }
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            // Cột Name dùng collation không dấu (ProductConfiguration) nên "may rua bat" / "dien" vẫn khớp
            var term = filter.Search.Trim();
            query = query.Where(x => x.p.Name.Contains(term) || x.t.Name.Contains(term));
        }

        var ordered = query.OrderBy(x => x.c.SortOrder).ThenBy(x => x.t.SortOrder).ThenBy(x => x.p.Name);
        var page = filter.Take is { } take ? ordered.Take(take) : ordered;

        return await page
            .Select(x => new ProductListItem(x.p, x.t.CategoryId, x.t.Name))
            .ToListAsync(cancellationToken);
    }
}
