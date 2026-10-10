using System.ComponentModel.DataAnnotations;
using BoschHomeVn.Contracts.Catalog;

namespace BoschHomeVn.Contracts.Admin;

// Sản phẩm cho trang quản trị — khác ProductResponse công khai: có giá nhập, trạng thái ẩn/hiện, cả sản phẩm chưa có giá
public sealed record AdminProductResponse(
    string Id,
    string Model,
    string Name,
    string CategoryId,
    string TypeId,
    string TypeName,
    int? Series,
    decimal? Price,
    decimal? OldPrice,
    decimal? DealerPrice,
    string StockStatus,
    int? StockQuantity,
    string? StockNote,
    string? Color,
    string? Origin,
    string? Warranty,
    bool IsNew,
    bool IsFlashSale,
    bool IsPublished,
    decimal? Rating,
    int ReviewCount,
    string? ImageUrl,
    IReadOnlyList<string> GalleryImages,
    string? VideoUrl,
    string? VideoPosterUrl,
    IReadOnlyList<string> Highlights,
    IReadOnlyList<ProductSpecResponse> Specs,
    bool AllowInstallment,
    IReadOnlyList<int> InstallmentMonths,
    int? InstallmentDisplayMonths);

public sealed record SaveProductRequest(
    [StringLength(32)] string Model,
    [StringLength(300)] string Name,
    [StringLength(64)] string TypeId,
    [Range(1, 9)] int? Series,
    decimal? Price,
    decimal? OldPrice,
    decimal? DealerPrice,
    string StockStatus,
    [Range(0, 1_000_000)] int? StockQuantity,
    [StringLength(200)] string? StockNote,
    [StringLength(100)] string? Color,
    [StringLength(100)] string? Origin,
    [StringLength(100)] string? Warranty,
    bool IsNew,
    bool IsFlashSale,
    bool IsPublished,
    [StringLength(500)] string? ImageUrl,
    IReadOnlyList<string>? GalleryImages,
    [StringLength(500)] string? VideoUrl,
    [StringLength(500)] string? VideoPosterUrl,
    IReadOnlyList<string> Highlights,
    IReadOnlyList<ProductSpecRequest> Specs,
    bool AllowInstallment,
    IReadOnlyList<int>? InstallmentMonths,
    int? InstallmentDisplayMonths);

public sealed record SaveProductArticleRequest([StringLength(200_000)] string? Html);

public sealed record ProductSpecRequest([StringLength(100)] string Name, [StringLength(300)] string Value);

public sealed record MediaResponse(string Url);
