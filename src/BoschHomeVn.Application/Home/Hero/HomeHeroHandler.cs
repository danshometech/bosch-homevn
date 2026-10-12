using System.Text.Encodings.Web;
using System.Text.Json;
using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Common;
using BoschHomeVn.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Home.Hero;

// Banner đầu trang chủ, lưu thành một chuỗi JSON trong SiteSettings (khóa home.hero; seed từ home.seed.json).
// Title: mỗi dòng là một dòng chữ lớn. Nút chỉ hiện khi có cả chữ và đường dẫn.
public sealed record HomeHero(
    string Time,
    string? Kicker,
    string Title,
    string? Lead,
    string? CtaLabel,
    string? CtaUrl,
    string ImageUrl,
    string? ImagePosition,
    string? ImageAlt)
{
    private static readonly JsonSerializerOptions Json = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public string ToJson() => JsonSerializer.Serialize(this, Json);

    public static HomeHero? FromJson(string json) => JsonSerializer.Deserialize<HomeHero>(json, Json);
}

public sealed class HomeHeroHandler(IAppDbContext db)
{
    public async Task<HomeHero?> GetAsync(CancellationToken cancellationToken)
    {
        var value = await db.SiteSettings
            .AsNoTracking()
            .Where(s => s.Key == SiteSetting.HomeHero)
            .Select(s => s.Value)
            .FirstOrDefaultAsync(cancellationToken);
        return value is null ? null : HomeHero.FromJson(value);
    }

    public async Task<HomeHero> SaveAsync(HomeHero input, CancellationToken cancellationToken)
    {
        var hero = Normalize(input);
        var value = hero.ToJson();
        if (value.Length > SiteSetting.MaxValueLength)
        {
            throw new DomainException("Nội dung banner quá dài, rút gọn lời dẫn hoặc tiêu đề.");
        }

        var row = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == SiteSetting.HomeHero, cancellationToken);
        if (row is null)
        {
            db.SiteSettings.Add(new SiteSetting(SiteSetting.HomeHero, value));
        }
        else
        {
            row.Set(value);
        }
        await db.SaveChangesAsync(cancellationToken);
        return hero;
    }

    private static HomeHero Normalize(HomeHero h)
    {
        var time = h.Time?.Trim() ?? "";
        if (!TimeOnly.TryParseExact(time, "HH:mm", out _))
        {
            throw new DomainException("Giờ phải có dạng HH:mm, vd. 06:32.");
        }

        var lines = (h.Title ?? "").Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            throw new DomainException("Banner cần tiêu đề.");
        }

        var ctaLabel = Clean(h.CtaLabel);
        var ctaUrl = Clean(h.CtaUrl);
        if ((ctaLabel is null) != (ctaUrl is null))
        {
            throw new DomainException("Nút cần cả chữ và đường dẫn — hoặc để trống cả hai để ẩn nút.");
        }

        var image = Clean(h.ImageUrl) ?? throw new DomainException("Banner cần ảnh nền.");
        return new HomeHero(time, Clean(h.Kicker), string.Join('\n', lines), Clean(h.Lead), ctaLabel, ctaUrl, image, Clean(h.ImagePosition), Clean(h.ImageAlt));
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
