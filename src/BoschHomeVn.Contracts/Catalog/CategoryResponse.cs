namespace BoschHomeVn.Contracts.Catalog;

// Dữ liệu menu/chân trang dùng chung mọi trang. FlashSaleCount = 0 thì ẩn link "Flash sale".
public sealed record MenuResponse(IReadOnlyList<CategoryResponse> Categories, int FlashSaleCount);

public sealed record CategoryResponse(string Id, string Name, string ShortName, IReadOnlyList<ProductTypeResponse> Types);

// Group: nhóm con trong danh mục (Thiết bị đun nấu…), null nếu danh mục không chia nhóm
public sealed record ProductTypeResponse(string Id, string Name, string? Group);
