using BoschHomeVn.Application.Abstractions.Content;
using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Catalog.Admin;

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

public sealed class ProductAdminHandler(IAppDbContext db, IArticleSanitizer sanitizer)
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

    public async Task<string?> GetArticleAsync(string id, CancellationToken cancellationToken)
    {
        if (!await db.Products.AnyAsync(p => p.Id == id, cancellationToken))
        {
            return null;
        }

        return await db.ProductArticles
            .Where(a => a.ProductId == id)
            .Select(a => a.Html)
            .FirstOrDefaultAsync(cancellationToken) ?? "";
    }

    public async Task<string?> SaveArticleAsync(string id, string? html, CancellationToken cancellationToken)
    {
        if (!await db.Products.AnyAsync(p => p.Id == id, cancellationToken))
        {
            return null;
        }

        var clean = sanitizer.Sanitize(html ?? "");
        var article = await db.ProductArticles.FirstOrDefaultAsync(a => a.ProductId == id, cancellationToken);
        if (clean.Length == 0)
        {
            if (article is not null)
            {
                db.ProductArticles.Remove(article);
            }
        }
        else if (article is null)
        {
            db.ProductArticles.Add(new ProductArticle(id, clean));
        }
        else
        {
            article.Update(clean);
        }

        await db.SaveChangesAsync(cancellationToken);
        return clean;
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
