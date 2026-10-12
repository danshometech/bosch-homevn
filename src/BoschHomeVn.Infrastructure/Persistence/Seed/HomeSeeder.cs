using System.Text.Json;
using BoschHomeVn.Application.Home.Hero;
using BoschHomeVn.Domain.Home;
using BoschHomeVn.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Infrastructure.Persistence.Seed;

// Seed từ home.seed.json: banner đầu trang khi chưa có khóa home.hero trong SiteSettings, các khoảnh khắc
// khi bảng HomeMoments trống (chuyển từ day.js của webstore).
// Chạy sau CatalogSeeder vì khoảnh khắc trỏ tới sản phẩm; mã sản phẩm không có trong DB thì bỏ qua.
internal static class HomeSeeder
{
    private const string ResourceName = "BoschHomeVn.Infrastructure.Persistence.Seed.home.seed.json";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void Seed(DbContext context)
    {
        var db = (AppDbContext)context;
        var needHero = !db.SiteSettings.Any(s => s.Key == SiteSetting.HomeHero);
        var needMoments = !db.HomeMoments.Any();
        if (!needHero && !needMoments)
        {
            return;
        }

        var seed = Load();
        if (needHero)
        {
            db.SiteSettings.Add(new SiteSetting(SiteSetting.HomeHero, seed.Hero.ToJson()));
        }
        if (needMoments)
        {
            var productIds = db.Products.Select(p => p.Id).ToHashSet();
            db.HomeMoments.AddRange(Build(seed, productIds));
        }
        db.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var db = (AppDbContext)context;
        var needHero = !await db.SiteSettings.AnyAsync(s => s.Key == SiteSetting.HomeHero, cancellationToken);
        var needMoments = !await db.HomeMoments.AnyAsync(cancellationToken);
        if (!needHero && !needMoments)
        {
            return;
        }

        var seed = Load();
        if (needHero)
        {
            db.SiteSettings.Add(new SiteSetting(SiteSetting.HomeHero, seed.Hero.ToJson()));
        }
        if (needMoments)
        {
            var productIds = (await db.Products.Select(p => p.Id).ToListAsync(cancellationToken)).ToHashSet();
            db.HomeMoments.AddRange(Build(seed, productIds));
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private static HomeSeed Load()
    {
        using var stream = typeof(HomeSeeder).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Không tìm thấy resource {ResourceName}.");
        return JsonSerializer.Deserialize<HomeSeed>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"{ResourceName} rỗng.");
    }

    private static IEnumerable<HomeMoment> Build(HomeSeed seed, HashSet<string> productIds) =>
        seed.Moments.Select((s, index) =>
        {
            var moment = new HomeMoment(s.Id, s.Time, s.Name, s.Tone, s.Title, s.Lead, index);
            moment.SetScene(s.ImageUrl, s.ImagePosition, s.StillImageUrl, s.ImageAlt);
            moment.SetFact(s.Fact.ProductId, s.Fact.SpecKey, s.Fact.Note);
            moment.SetLinks(s.Links.Select(l => new MomentLink(l.Label, l.Url)));
            moment.SetExtras(s.Extras.Title, s.Extras.MoreLabel, s.Extras.MoreUrl);
            foreach (var id in s.Products.Where(productIds.Contains))
            {
                moment.AddProduct(id, MomentSlot.Main);
            }
            foreach (var id in s.Extras.ProductIds.Where(productIds.Contains))
            {
                moment.AddProduct(id, MomentSlot.Extra);
            }
            return moment;
        });

    private sealed record HomeSeed(HomeHero Hero, List<MomentSeed> Moments);

    private sealed record MomentSeed(
        string Id,
        string Time,
        string Name,
        string Tone,
        string Title,
        string Lead,
        string? ImageUrl,
        string? ImagePosition,
        string? StillImageUrl,
        string? ImageAlt,
        FactSeed Fact,
        List<LinkSeed> Links,
        List<string> Products,
        ExtrasSeed Extras);

    private sealed record FactSeed(string? ProductId, string? SpecKey, string? Note);

    private sealed record LinkSeed(string Label, string Url);

    private sealed record ExtrasSeed(string? Title, List<string> ProductIds, string? MoreLabel, string? MoreUrl);
}
