using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Catalog.GetMenu;

// FlashSaleCount = số sản phẩm flash sale đang bán, 0 thì site ẩn link "Flash sale"
public sealed record Menu(IReadOnlyList<Category> Categories, int FlashSaleCount);

// Dữ liệu cho menu/chân trang dùng chung mọi trang: cây danh mục đúng thứ tự hiển thị
public sealed class GetMenuHandler(IAppDbContext db)
{
    public async Task<Menu> Handle(CancellationToken cancellationToken)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .Include(c => c.Types.OrderBy(t => t.SortOrder))
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
        var flashSaleCount = await db.Products.CountAsync(p => p.Price != null && p.IsPublished && p.IsFlashSale, cancellationToken);

        return new Menu(categories, flashSaleCount);
    }
}
