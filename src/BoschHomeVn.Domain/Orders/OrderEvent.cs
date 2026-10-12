namespace BoschHomeVn.Domain.Orders;

// Một dòng lịch sử xử lý đơn: khách đặt hàng (Actor null), admin đổi trạng thái (FromStatus → ToStatus),
// đánh dấu / bỏ đánh dấu đã thu tiền (IsPaid), hoặc chỉ thêm ghi chú. Trường nào không đổi thì null
public sealed class OrderEvent
{
    private OrderEvent() { } // EF Core

    internal OrderEvent(Guid orderId, DateTime createdAt, string? actor, OrderStatus? fromStatus, OrderStatus? toStatus, bool? isPaid, string? note)
    {
        OrderId = orderId;
        CreatedAt = createdAt;
        Actor = actor;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        IsPaid = isPaid;
        Note = note;
    }

    public long Id { get; private set; }
    public Guid OrderId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    // Tài khoản admin thao tác; null = khách (lúc đặt hàng)
    public string? Actor { get; private set; }
    public OrderStatus? FromStatus { get; private set; }
    public OrderStatus? ToStatus { get; private set; }
    public bool? IsPaid { get; private set; }
    public string? Note { get; private set; }
}
