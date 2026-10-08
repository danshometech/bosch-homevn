using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Application.Catalog;
using BoschHomeVn.Domain.Home;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Home.GetHomePage;

public sealed record HomeMomentItem(
    HomeMoment Moment,
    IReadOnlyList<ProductListItem> Products,
    IReadOnlyList<ProductListItem> Extras,
    string? FactValue);

public sealed record FlashSaleSummary(int Count, int MaxDiscountPercent, IReadOnlyList<string> TypeNames);

public sealed record HomePage(IReadOnlyList<HomeMomentItem> Moments, FlashSaleSummary? FlashSale);

// Dữ liệu trang chủ trong một lần gọi: các khoảnh khắc kèm sản phẩm đang bán, và tóm tắt flash sale.
// Khoảnh khắc không còn sản phẩm nào đang bán thì bỏ qua.
public sealed class GetHomePageHandler(IAppDbContext db)
{
    public async Task<HomePage> Handle(CancellationToken cancellationToken)
    {
        var moments = await db.HomeMoments
            .AsNoTracking()
            .Include(m => m.Products)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);

        var ids = moments
            .SelectMany(m => m.Products.Select(p => p.ProductId))
            .Concat(moments.Select(m => m.FactProductId).OfType<string>())
            .Distinct()
            .ToList();
        var products = (await db.SellableProductsAsync(new ProductFilter(Ids: ids), cancellationToken))
            .ToDictionary(x => x.Product.Id);

        IReadOnlyList<ProductListItem> Pick(HomeMoment m, MomentSlot slot) =>
        [
            .. m.Products
                .Where(p => p.Slot == slot)
                .OrderBy(p => p.SortOrder)
                .Select(p => products.GetValueOrDefault(p.ProductId))
                .OfType<ProductListItem>(),
        ];

        string? FactValue(HomeMoment m) =>
            m.FactProductId is { } id && products.TryGetValue(id, out var item)
                ? item.Product.Specs.FirstOrDefault(s => s.Name == m.FactSpecKey)?.Value
                : null;

        var items = moments
            .Select(m => new HomeMomentItem(m, Pick(m, MomentSlot.Main), Pick(m, MomentSlot.Extra), FactValue(m)))
            .Where(x => x.Products.Count > 0)
            .ToList();

        return new HomePage(items, await GetFlashSaleAsync(cancellationToken));
    }

    private async Task<FlashSaleSummary?> GetFlashSaleAsync(CancellationToken cancellationToken)
    {
        var flash = await (
            from p in db.Products
            join t in db.ProductTypes on p.ProductTypeId equals t.Id
            where p.Price != null && p.IsPublished && p.IsFlashSale
            select new { Price = p.Price!.Value, p.OldPrice, Type = t.Name })
            .ToListAsync(cancellationToken);
        if (flash.Count == 0)
        {
            return null;
        }

        var maxDiscount = flash
            .Where(x => x.OldPrice > x.Price)
            .Select(x => (int)Math.Round((1 - x.Price / x.OldPrice!.Value) * 100))
            .DefaultIfEmpty(0)
            .Max();
        return new FlashSaleSummary(flash.Count, maxDiscount, [.. flash.Select(x => x.Type).Distinct()]);
    }
}
