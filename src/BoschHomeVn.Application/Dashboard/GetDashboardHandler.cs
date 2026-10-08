using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Application.Catalog;
using BoschHomeVn.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Dashboard;

public sealed record CategoryStat(string Id, string Name, int Total, int OnSale);

// OnSale = đang hiện trên site (có giá bán + đang bật); NoPrice = chưa có giá bán; Hidden = admin đang tắt
public sealed record DashboardData(
    int Total,
    int OnSale,
    int NoPrice,
    int Hidden,
    int LowStock,
    int OutOfStock,
    IReadOnlyList<CategoryStat> Categories,
    IReadOnlyList<ProductListItem> OutOfStockProducts);

public sealed class GetDashboardHandler(IAppDbContext db)
{
    public async Task<DashboardData> Handle(CancellationToken cancellationToken)
    {
        var rows = await (
            from p in db.Products
            join t in db.ProductTypes on p.ProductTypeId equals t.Id
            join c in db.Categories on t.CategoryId equals c.Id
            select new { c.Id, c.Name, c.SortOrder, OnSale = p.Price != null && p.IsPublished, NoPrice = p.Price == null, Hidden = !p.IsPublished, p.StockStatus })
            .ToListAsync(cancellationToken);

        var categories = rows
            .GroupBy(r => new { r.Id, r.Name, r.SortOrder })
            .OrderBy(g => g.Key.SortOrder)
            .Select(g => new CategoryStat(g.Key.Id, g.Key.Name, g.Count(), g.Count(r => r.OnSale)))
            .ToList();

        var outOfStock = await (
            from p in db.Products.AsNoTracking()
            where p.StockStatus == StockStatus.OutOfStock
            join t in db.ProductTypes on p.ProductTypeId equals t.Id
            orderby p.Name
            select new ProductListItem(p, t.CategoryId, t.Name))
            .ToListAsync(cancellationToken);

        return new DashboardData(
            rows.Count,
            rows.Count(r => r.OnSale),
            rows.Count(r => r.NoPrice),
            rows.Count(r => r.Hidden),
            rows.Count(r => r.StockStatus == StockStatus.LowStock),
            rows.Count(r => r.StockStatus == StockStatus.OutOfStock),
            categories,
            outOfStock);
    }
}
