namespace BoschHomeVn.Contracts.Admin;

// OnSale: đang hiện trên site · NoPrice: chưa có giá bán · Hidden: đang tắt hiển thị
public sealed record DashboardResponse(
    int Total,
    int OnSale,
    int NoPrice,
    int Hidden,
    int LowStock,
    int OutOfStock,
    IReadOnlyList<DashboardCategoryResponse> Categories,
    IReadOnlyList<AdminProductResponse> OutOfStockProducts);

public sealed record DashboardCategoryResponse(string Id, string Name, int Total, int OnSale);
