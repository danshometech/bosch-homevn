using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Common;
using BoschHomeVn.Domain.Home;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Home.Admin;

public sealed record SaveMomentCommand(
    string Time,
    string Name,
    string Tone,
    string Title,
    string Lead,
    string? ImageUrl,
    string? ImagePosition,
    string? StillImageUrl,
    string? ImageAlt,
    string? FactProductId,
    string? FactSpecKey,
    string? FactNote,
    IReadOnlyList<MomentLink> Links,
    string? ExtrasTitle,
    string? ExtrasMoreLabel,
    string? ExtrasMoreUrl,
    IReadOnlyList<string> ProductIds,
    IReadOnlyList<string> ExtraProductIds);

// Quản trị khoảnh khắc trang chủ: xem tất cả (kể cả khoảnh khắc chưa có sản phẩm nên đang ẩn) và sửa
public sealed class MomentAdminHandler(IAppDbContext db)
{
    public async Task<IReadOnlyList<HomeMoment>> ListAsync(CancellationToken cancellationToken) =>
        await db.HomeMoments
            .AsNoTracking()
            .Include(m => m.Products)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<HomeMoment?> GetAsync(string id, CancellationToken cancellationToken) =>
        await db.HomeMoments
            .AsNoTracking()
            .Include(m => m.Products)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<bool> UpdateAsync(string id, SaveMomentCommand c, CancellationToken cancellationToken)
    {
        var moment = await db.HomeMoments.Include(m => m.Products).FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (moment is null)
        {
            return false;
        }

        var ids = c.ProductIds.Concat(c.ExtraProductIds).Append(c.FactProductId).OfType<string>().Distinct().ToList();
        var found = await db.Products.Where(p => ids.Contains(p.Id)).Select(p => p.Id).ToListAsync(cancellationToken);
        var missing = ids.Except(found).ToList();
        if (missing.Count > 0)
        {
            throw new DomainException($"Không có sản phẩm: {string.Join(", ", missing)}.");
        }

        moment.Update(c.Time, c.Name, c.Tone, c.Title, c.Lead);
        moment.SetScene(Clean(c.ImageUrl), Clean(c.ImagePosition), Clean(c.StillImageUrl), Clean(c.ImageAlt));
        moment.SetFact(Clean(c.FactProductId), Clean(c.FactSpecKey), Clean(c.FactNote));
        moment.SetLinks(c.Links.Where(l => !string.IsNullOrWhiteSpace(l.Label) && !string.IsNullOrWhiteSpace(l.Url))
            .Select(l => new MomentLink(l.Label.Trim(), l.Url.Trim())));
        moment.SetExtras(Clean(c.ExtrasTitle), Clean(c.ExtrasMoreLabel), Clean(c.ExtrasMoreUrl));
        moment.SetProducts(c.ProductIds, c.ExtraProductIds);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
