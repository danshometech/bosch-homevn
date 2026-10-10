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

public enum ProductSort { Popular, Discount, PriceAsc, PriceDesc }

// Trang danh sách: phạm vi (danh mục / từ khóa / flash sale) + bộ lọc loại, giá [MinPrice, MaxPrice), đánh giá
public sealed record ProductListingQuery(
    string? CategoryId,
    string? Search,
    bool FlashSaleOnly,
    IReadOnlyCollection<string>? TypeNames,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? MinRating,
    ProductSort Sort,
    int Page,
    int PageSize);

public sealed record ProductTypeCount(string TypeName, int Count);

// TypeCounts: số sản phẩm mỗi loại trong phạm vi, không tính bộ lọc loại / giá / đánh giá (cho cây "Loại sản phẩm")
public sealed record ProductListingPage(IReadOnlyList<ProductListItem> Items, int Page, int Total, IReadOnlyList<ProductTypeCount> TypeCounts);

internal static class ProductQueries
{
    private sealed class Row
    {
        public required Product P { get; init; }
        public required ProductType T { get; init; }
        public required Category C { get; init; }
    }

    public static async Task<IReadOnlyList<ProductListItem>> SellableProductsAsync(
        this IAppDbContext db, ProductFilter filter, CancellationToken cancellationToken)
    {
        var query = Scope(db, filter.CategoryId, filter.Search, filter.FlashSaleOnly);
        if (filter.Ids is { } ids)
        {
            query = query.Where(x => ids.Contains(x.P.Id));
        }

        var ordered = MenuOrder(query);
        var page = filter.Take is { } take ? ordered.Take(take) : ordered;
        return await page.Select(x => new ProductListItem(x.P, x.T.CategoryId, x.T.Name)).ToListAsync(cancellationToken);
    }

    public static async Task<ProductListingPage> ProductListingAsync(
        this IAppDbContext db, ProductListingQuery q, CancellationToken cancellationToken)
    {
        var scope = Scope(db, q.CategoryId, q.Search, q.FlashSaleOnly);
        var typeCounts = await scope
            .GroupBy(x => x.T.Name)
            .Select(g => new ProductTypeCount(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var query = scope;
        if (q.TypeNames is { Count: > 0 } types)
        {
            query = query.Where(x => types.Contains(x.T.Name));
        }
        if (q.MinPrice is { } min)
        {
            query = query.Where(x => x.P.Price >= min);
        }
        if (q.MaxPrice is { } max)
        {
            query = query.Where(x => x.P.Price < max);
        }
        if (q.MinRating is { } rating)
        {
            query = query.Where(x => (x.P.Rating ?? 0) >= rating);
        }

        var total = await query.CountAsync(cancellationToken);
        var page = Math.Clamp(q.Page, 1, Math.Max(1, (int)Math.Ceiling(total / (double)q.PageSize)));
        var items = await Sorted(query, q.Sort)
            .Skip((page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(x => new ProductListItem(x.P, x.T.CategoryId, x.T.Name))
            .ToListAsync(cancellationToken);
        return new ProductListingPage(items, page, total, typeCounts);
    }

    // Sản phẩm đang bán trên site: đã có giá bán (Price null = chờ admin nhập giá) và đang bật hiển thị
    private static IQueryable<Row> Scope(IAppDbContext db, string? categoryId, string? search, bool flashSaleOnly)
    {
        var query =
            from p in db.Products.AsNoTracking()
            join t in db.ProductTypes on p.ProductTypeId equals t.Id
            join c in db.Categories on t.CategoryId equals c.Id
            where p.Price != null && p.IsPublished
            select new Row { P = p, T = t, C = c };

        if (categoryId is not null)
        {
            query = query.Where(x => x.T.CategoryId == categoryId);
        }
        if (flashSaleOnly)
        {
            query = query.Where(x => x.P.IsFlashSale);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = SearchText.Normalize(search);
            query = query.Where(x => SearchText.Fold(x.P.Name).Contains(term) || SearchText.Fold(x.T.Name).Contains(term));
        }
        return query;
    }

    private static IOrderedQueryable<Row> MenuOrder(IQueryable<Row> query) =>
        query.OrderBy(x => x.C.SortOrder).ThenBy(x => x.T.SortOrder).ThenBy(x => x.P.Name);

    // Cùng giá trị sắp xếp thì theo thứ tự menu, cuối cùng theo mã để phân trang không lặp / sót
    private static IOrderedQueryable<Row> Sorted(IQueryable<Row> query, ProductSort sort)
    {
        var ordered = sort switch
        {
            ProductSort.Discount => query.OrderByDescending(x => x.P.OldPrice > x.P.Price ? (x.P.OldPrice - x.P.Price) / x.P.OldPrice : 0),
            ProductSort.PriceAsc => query.OrderBy(x => x.P.Price),
            ProductSort.PriceDesc => query.OrderByDescending(x => x.P.Price),
            _ => query.OrderByDescending(x => x.P.ReviewCount),
        };
        return ordered.ThenBy(x => x.C.SortOrder).ThenBy(x => x.T.SortOrder).ThenBy(x => x.P.Name).ThenBy(x => x.P.Id);
    }
}
