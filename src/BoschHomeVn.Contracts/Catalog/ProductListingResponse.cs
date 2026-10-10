namespace BoschHomeVn.Contracts.Catalog;

// TypeCounts: số sản phẩm mỗi loại trong phạm vi đang xem, không tính bộ lọc loại / giá / đánh giá
public sealed record ProductListingResponse(
    IReadOnlyList<ProductResponse> Items,
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<ProductTypeCountResponse> TypeCounts);

public sealed record ProductTypeCountResponse(string Name, int Count);
