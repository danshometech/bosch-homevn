using System.Text.Json;
using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.News;
using BoschHomeVn.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Infrastructure.Persistence.Seed;

// Seed từ content.seed.json, chỉ chạy khi DB mới tạo (CatalogSeeder vừa nạp sản phẩm): liên hệ, bài tin viết trong admin
// (bài mẫu nằm ở migration SeedNewsSamples), bài giới thiệu sản phẩm.
internal static class ContentSeeder
{
    private const string ResourceName = "BoschHomeVn.Infrastructure.Persistence.Seed.content.seed.json";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void Seed(DbContext context)
    {
        var db = (AppDbContext)context;
        var categoryIds = db.NewsCategories.Select(c => c.Id).ToHashSet();
        var productIds = db.Products.Select(p => p.Id).ToHashSet();
        var posts = db.NewsPosts.ToDictionary(p => p.Slug);
        Add(db, Load(), categoryIds, productIds, posts);
        db.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var db = (AppDbContext)context;
        var categoryIds = (await db.NewsCategories.Select(c => c.Id).ToListAsync(cancellationToken)).ToHashSet();
        var productIds = (await db.Products.Select(p => p.Id).ToListAsync(cancellationToken)).ToHashSet();
        var posts = await db.NewsPosts.ToDictionaryAsync(p => p.Slug, cancellationToken);
        Add(db, Load(), categoryIds, productIds, posts);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static ContentSeed Load()
    {
        using var stream = typeof(ContentSeeder).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Không tìm thấy resource {ResourceName}.");
        return JsonSerializer.Deserialize<ContentSeed>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"{ResourceName} rỗng.");
    }

    private static void Add(
        AppDbContext db, ContentSeed seed, HashSet<string> categoryIds, HashSet<string> productIds, Dictionary<string, NewsPost> posts)
    {
        var c = seed.Contact;
        foreach (var (key, value) in new[]
        {
            (SiteSetting.Hotline, c.Hotline),
            (SiteSetting.ZaloUrl, c.ZaloUrl),
            (SiteSetting.FacebookUrl, c.FacebookUrl),
            (SiteSetting.TikTokUrl, c.TikTokUrl),
            (SiteSetting.YouTubeUrl, c.YouTubeUrl),
        })
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                db.SiteSettings.Add(new SiteSetting(key, value));
            }
        }

        // Bài trùng slug với bài mẫu của migration thì sửa theo seed
        foreach (var n in seed.News.Where(n => categoryIds.Contains(n.CategoryId)))
        {
            if (posts.TryGetValue(n.Slug, out var post))
            {
                post.Edit(n.CategoryId, n.Title, n.Slug, n.Summary, n.CoverImageUrl, n.Html, n.UpdatedAt);
            }
            else
            {
                post = new NewsPost(n.CategoryId, n.Title, n.Slug, n.Summary, n.CoverImageUrl, n.Html, n.UpdatedAt);
                db.NewsPosts.Add(post);
            }

            if (n.IsPublished)
            {
                post.Publish(n.PublishedAt ?? n.UpdatedAt);
            }
            else
            {
                post.Unpublish();
            }
        }

        foreach (var a in seed.Articles.Where(a => productIds.Contains(a.ProductId)))
        {
            db.ProductArticles.Add(new ProductArticle(a.ProductId, a.Html));
        }
    }

    private sealed record ContentSeed(ContactSeed Contact, List<NewsSeed> News, List<ArticleSeed> Articles);

    private sealed record ContactSeed(string? Hotline, string? ZaloUrl, string? FacebookUrl, string? TikTokUrl, string? YouTubeUrl);

    private sealed record NewsSeed(
        string CategoryId,
        string Title,
        string Slug,
        string? Summary,
        string? CoverImageUrl,
        string Html,
        bool IsPublished,
        DateTime? PublishedAt,
        DateTime UpdatedAt);

    private sealed record ArticleSeed(string ProductId, string Html);
}
