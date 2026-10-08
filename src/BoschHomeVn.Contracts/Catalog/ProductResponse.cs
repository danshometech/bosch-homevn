namespace BoschHomeVn.Contracts.Catalog;

// Sản phẩm trả cho site bán hàng — không có giá nhập đại lý
public sealed record ProductResponse(
    string Id,
    string Model,
    string Name,
    string CategoryId,
    string TypeId,
    string TypeName,
    int? Series,
    decimal Price,
    decimal? OldPrice,
    string StockStatus,
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
    IReadOnlyList<string> GalleryImages,
    string? VideoUrl,
    string? VideoPosterUrl,
    IReadOnlyList<string> Highlights,
    IReadOnlyList<ProductSpecResponse> Specs,
    ProductInstallmentResponse? Installment);

public sealed record ProductSpecResponse(string Name, string Value);

// Trả góp 0% của sản phẩm; null = không trả góp
public sealed record ProductInstallmentResponse(IReadOnlyList<int> Months, int DisplayMonths);
