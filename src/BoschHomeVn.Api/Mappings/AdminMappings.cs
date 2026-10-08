using BoschHomeVn.Application.Catalog;
using BoschHomeVn.Application.Catalog.Admin;
using BoschHomeVn.Application.Home.Admin;
using BoschHomeVn.Contracts.Admin;
using BoschHomeVn.Contracts.Catalog;
using BoschHomeVn.Contracts.Home;
using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Common;
using BoschHomeVn.Domain.Home;

namespace BoschHomeVn.Api.Mappings;

internal static class AdminMappings
{
    public static AdminProductResponse ToAdminResponse(this ProductListItem item)
    {
        var p = item.Product;
        return new AdminProductResponse(
            p.Id,
            p.Model,
            p.Name,
            item.CategoryId,
            p.ProductTypeId,
            item.TypeName,
            p.Series,
            p.Price,
            p.OldPrice,
            p.DealerPrice,
            p.StockStatus.ToString(),
            p.StockQuantity,
            p.StockNote,
            p.Color,
            p.Origin,
            p.Warranty,
            p.IsNew,
            p.IsFlashSale,
            p.IsPublished,
            p.Rating,
            p.ReviewCount,
            p.ImageUrl,
            p.GalleryImages,
            p.VideoUrl,
            p.VideoPosterUrl,
            p.Highlights,
            [.. p.Specs.Select(s => new ProductSpecResponse(s.Name, s.Value))],
            p.AllowInstallment,
            p.InstallmentMonths,
            p.InstallmentDisplayMonths);
    }

    public static SaveProductCommand ToCommand(this SaveProductRequest r) =>
        new(
            r.Model,
            r.Name,
            r.TypeId,
            r.Series,
            r.Price,
            r.OldPrice,
            r.DealerPrice,
            Enum.TryParse<StockStatus>(r.StockStatus, out var status) && Enum.IsDefined(status)
                ? status
                : throw new DomainException("Tình trạng hàng phải là InStock, LowStock hoặc OutOfStock."),
            r.StockQuantity,
            r.StockNote,
            r.Color,
            r.Origin,
            r.Warranty,
            r.IsNew,
            r.IsFlashSale,
            r.IsPublished,
            r.ImageUrl,
            r.GalleryImages,
            r.VideoUrl,
            r.VideoPosterUrl,
            r.Highlights ?? [],
            [.. (r.Specs ?? []).Select(s => new ProductSpec(s.Name ?? "", s.Value ?? ""))],
            r.AllowInstallment,
            r.InstallmentMonths ?? [],
            r.InstallmentDisplayMonths);

    public static IReadOnlyList<AdminCategoryResponse> ToResponse(this CategoryTree tree) =>
    [
        .. tree.Categories.Select(c => new AdminCategoryResponse(
            c.Id,
            c.Name,
            c.ShortName,
            [.. c.Types.Select(t => new AdminProductTypeResponse(t.Id, t.Name, t.GroupName, tree.ProductCountByType.GetValueOrDefault(t.Id)))])),
    ];

    public static AdminMomentResponse ToAdminResponse(this HomeMoment m) =>
        new(
            m.Id,
            m.Time,
            m.Name,
            m.Tone,
            m.Title,
            m.Lead,
            m.ImageUrl,
            m.ImagePosition,
            m.StillImageUrl,
            m.ImageAlt,
            m.FactProductId,
            m.FactSpecKey,
            m.FactNote,
            [.. m.Links.Select(l => new LinkResponse(l.Label, l.Url))],
            m.ExtrasTitle,
            m.ExtrasMoreLabel,
            m.ExtrasMoreUrl,
            [.. m.Products.Where(p => p.Slot == MomentSlot.Main).OrderBy(p => p.SortOrder).Select(p => p.ProductId)],
            [.. m.Products.Where(p => p.Slot == MomentSlot.Extra).OrderBy(p => p.SortOrder).Select(p => p.ProductId)],
            HomeMoment.Tones);

    public static SaveMomentCommand ToCommand(this SaveMomentRequest r) =>
        new(
            r.Time,
            r.Name,
            r.Tone,
            r.Title,
            r.Lead ?? "",
            r.ImageUrl,
            r.ImagePosition,
            r.StillImageUrl,
            r.ImageAlt,
            r.FactProductId,
            r.FactSpecKey,
            r.FactNote,
            [.. (r.Links ?? []).Select(l => new MomentLink(l.Label ?? "", l.Url ?? ""))],
            r.ExtrasTitle,
            r.ExtrasMoreLabel,
            r.ExtrasMoreUrl,
            r.ProductIds ?? [],
            r.ExtraProductIds ?? []);
}
