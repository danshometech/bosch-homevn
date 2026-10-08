using System.Text.Json;
using System.Text.Json.Serialization;
using BoschHomeVn.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Infrastructure.Persistence.Seed;

// Seed 1 lần khi DB trống: danh mục + sản phẩm từ catalog.seed.json (sản phẩm từ data.xlsx, giá bán = giá nhập).
// Gọi qua UseSeeding/UseAsyncSeeding → chạy khi `dotnet ef database update` hoặc Database.Migrate().
internal static class CatalogSeeder
{
    private const string ResourceName = "BoschHomeVn.Infrastructure.Persistence.Seed.catalog.seed.json";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static void Seed(DbContext context)
    {
        var db = (AppDbContext)context;
        var seed = Load();
        var changed = false;

        if (!db.Categories.Any())
        {
            db.Categories.AddRange(BuildCategories(seed));
            changed = true;
        }
        if (!db.Products.Any())
        {
            db.Products.AddRange(BuildProducts(seed));
            changed = true;
        }

        if (changed)
        {
            db.SaveChanges();
        }
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var db = (AppDbContext)context;
        var seed = Load();
        var changed = false;

        if (!await db.Categories.AnyAsync(cancellationToken))
        {
            db.Categories.AddRange(BuildCategories(seed));
            changed = true;
        }
        if (!await db.Products.AnyAsync(cancellationToken))
        {
            db.Products.AddRange(BuildProducts(seed));
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static CatalogSeed Load()
    {
        using var stream = typeof(CatalogSeeder).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Không tìm thấy resource {ResourceName}.");
        return JsonSerializer.Deserialize<CatalogSeed>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"{ResourceName} rỗng.");
    }

    private static IEnumerable<Category> BuildCategories(CatalogSeed seed) =>
        seed.Categories.Select((c, index) =>
        {
            var category = new Category(c.Id, c.Name, c.ShortName, index);
            foreach (var t in c.Types)
            {
                category.AddType(t.Id, t.Name, t.Group);
            }
            return category;
        });

    private static IEnumerable<Product> BuildProducts(CatalogSeed seed) =>
        seed.Products.Select(s =>
        {
            var product = new Product(s.Model, s.Name, s.Type);
            product.SetDetails(s.Series, s.Color, s.Origin, s.Warranty);
            product.SetPrice(s.Price, s.OldPrice);
            product.SetDealerPrice(s.DealerPrice);
            product.SetStock(s.StockStatus, s.StockQuantity, s.StockNote);
            product.SetRating(s.Rating, s.ReviewCount);
            product.SetDisplay(s.IsNew, s.IsFlashSale, s.ImageUrl, s.Highlights,
                s.Specs.Select(x => new ProductSpec(x.Name, x.Value)));
            return product;
        });

    private sealed record CatalogSeed(List<CategorySeed> Categories, List<ProductSeed> Products);

    private sealed record CategorySeed(string Id, string Name, string ShortName, List<TypeSeed> Types);

    private sealed record TypeSeed(string Id, string Name, string? Group);

    private sealed record ProductSeed(
        string Model,
        string Name,
        string Type,
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
        decimal? Rating,
        int ReviewCount,
        string? ImageUrl,
        List<string> Highlights,
        List<SpecSeed> Specs);

    private sealed record SpecSeed(string Name, string Value);
}
