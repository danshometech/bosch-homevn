using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Catalog.Admin;

// Model chỉ dùng khi thêm mới (Id = model viết thường, không đổi được); Rating / ReviewCount không sửa ở đây
public sealed record SaveProductCommand(
    string Model,
    string Name,
    string TypeId,
    int? Series,
    decimal? Price,
    decimal? OldPrice,
    decimal? DealerPrice,
    StockStatus StockStatus,
    int? StockQuantity,
    string? StockNote,
    string? Color,
    string? Origin,
    string? Warranty,
    bool IsNew,
    bool IsFlashSale,
    bool IsPublished,
    string? ImageUrl,
    IReadOnlyList<string>? GalleryImages,
    string? VideoUrl,
    string? VideoPosterUrl,
    IReadOnlyList<string> Highlights,
    IReadOnlyList<ProductSpec> Specs,
    bool AllowInstallment,
    IReadOnlyList<int> InstallmentMonths,
    int? InstallmentDisplayMonths);

// Quản trị sản phẩm: xem tất cả (kể cả chưa có giá / đang ẩn, có giá nhập), thêm, sửa, xóa
public sealed class ProductAdminHandler(IAppDbContext db)
{
    public async Task<IReadOnlyList<ProductListItem>> ListAsync(CancellationToken cancellationToken) =>
        await (
            from p in db.Products.AsNoTracking()
            join t in db.ProductTypes on p.ProductTypeId equals t.Id
            join c in db.Categories on t.CategoryId equals c.Id
            orderby c.SortOrder, t.SortOrder, p.Name
            select new ProductListItem(p, t.CategoryId, t.Name))
            .ToListAsync(cancellationToken);

    public async Task<ProductListItem?> GetAsync(string id, CancellationToken cancellationToken) =>
        await (
            from p in db.Products.AsNoTracking()
            where p.Id == id
            join t in db.ProductTypes on p.ProductTypeId equals t.Id
            select new ProductListItem(p, t.CategoryId, t.Name))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<string> CreateAsync(SaveProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product(command.Model, command.Name, command.TypeId);
        if (await db.Products.AnyAsync(p => p.Id == product.Id, cancellationToken))
        {
            throw new DomainException($"Mã {product.Model} đã có.");
        }

        await EnsureTypeAsync(command.TypeId, cancellationToken);
        Apply(product, command);
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return product.Id;
    }

    public async Task<bool> UpdateAsync(string id, SaveProductCommand command, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        await EnsureTypeAsync(command.TypeId, cancellationToken);
        product.Rename(command.Name);
        product.ChangeType(command.TypeId);
        Apply(product, command);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // Xóa hẳn; sản phẩm tự gỡ khỏi các khoảnh khắc trang chủ (khóa ngoại cascade)
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureTypeAsync(string typeId, CancellationToken cancellationToken)
    {
        if (!await db.ProductTypes.AnyAsync(t => t.Id == typeId, cancellationToken))
        {
            throw new DomainException($"Loại sản phẩm \"{typeId}\" không tồn tại.");
        }
    }

    private static void Apply(Product product, SaveProductCommand c)
    {
        product.SetDetails(c.Series, Clean(c.Color), Clean(c.Origin), Clean(c.Warranty));
        product.SetPrice(c.Price, c.OldPrice);
        product.SetDealerPrice(c.DealerPrice);
        product.SetStock(c.StockStatus, c.StockQuantity, c.StockNote);
        product.SetPublished(c.IsPublished);
        // Request không gửi ảnh thêm (null) thì giữ nguyên, tránh xóa nhầm
        if (c.GalleryImages is not null)
        {
            product.SetGallery(c.GalleryImages);
        }
        product.SetVideo(c.VideoUrl, c.VideoPosterUrl);
        product.SetInstallment(c.AllowInstallment, c.InstallmentMonths, c.InstallmentDisplayMonths);
        product.SetDisplay(
            c.IsNew,
            c.IsFlashSale,
            Clean(c.ImageUrl),
            c.Highlights.Select(h => h.Trim()).Where(h => h.Length > 0),
            c.Specs
                .Where(s => !string.IsNullOrWhiteSpace(s.Name) && !string.IsNullOrWhiteSpace(s.Value))
                .Select(s => new ProductSpec(s.Name.Trim(), s.Value.Trim())));
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
