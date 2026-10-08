namespace BoschHomeVn.Contracts.Common;

// Kết quả có phân trang trả về cho client (danh sách sản phẩm, đơn hàng…)
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
