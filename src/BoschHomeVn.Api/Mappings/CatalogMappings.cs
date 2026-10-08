using BoschHomeVn.Application.Catalog;
using BoschHomeVn.Contracts.Catalog;
using BoschHomeVn.Domain.Catalog;

namespace BoschHomeVn.Api.Mappings;

internal static class CatalogMappings
{
    public static CategoryResponse ToResponse(this Category c) =>
        new(c.Id, c.Name, c.ShortName, [.. c.Types.Select(t => new ProductTypeResponse(t.Id, t.Name, t.GroupName))]);

    // Map tay từng trường: DealerPrice (giá nhập) cố ý không có ở đây
    public static ProductResponse ToResponse(this ProductListItem item)
    {
        var p = item.Product;
        return new ProductResponse(
            p.Id,
            p.Model,
            p.Name,
            item.CategoryId,
            p.ProductTypeId,
            item.TypeName,
            p.Series,
            p.Price!.Value,
            p.OldPrice,
            p.StockStatus.ToString(),
            p.StockQuantity,
            p.StockNote,
            p.Color,
            p.Origin,
            p.Warranty,
            p.IsNew,
            p.IsFlashSale,
            p.Rating,
            p.ReviewCount,
            p.ImageUrl,
            p.GalleryImages,
            p.VideoUrl,
            p.VideoPosterUrl,
            p.Highlights,
            [.. p.Specs.Select(s => new ProductSpecResponse(s.Name, s.Value))],
            p.AllowInstallment && p.InstallmentDisplayMonths is int display
                ? new ProductInstallmentResponse(p.InstallmentMonths, display)
                : null);
    }
}
