namespace BoschHomeVn.Domain.Common;

// Lớp gốc của mọi entity. Kiểu Id tuỳ entity: mã sản phẩm là chuỗi ("smv4hcx48e"), đơn hàng có thể là Guid
public abstract class Entity<TId>
    where TId : notnull
{
    public TId Id { get; protected init; } = default!;
}
