using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Domain.Orders;

public enum OrderStatus
{
    New,
    Confirmed,
    Shipping,
    Completed,
    Cancelled,
}

// Thông tin người nhận khách điền khi đặt hàng
public sealed record OrderContact(string Name, string Phone, string? Email, string? Province, string? Ward, string Address, string? Note);

// Đơn hàng khách đặt trên site. Tên / giá sản phẩm, phí giao, tên cách giao và thanh toán lưu lại đúng lúc đặt —
// sau này đổi giá sản phẩm hay sửa cách giao hàng thì đơn cũ không đổi. Code: mã đơn khách thấy (vd. BH2610120427).
// Events: lịch sử xử lý, đổi trạng thái bắt buộc kèm ghi chú
public sealed class Order : Entity<Guid>
{
    public const int MaxNoteLength = 1000;

    private readonly List<OrderItem> _items = [];
    private readonly List<OrderEvent> _events = [];

    private Order() { } // EF Core

    public Order(
        string code,
        OrderContact contact,
        IEnumerable<OrderItem> items,
        string shippingMethodId,
        string shippingMethodName,
        decimal shippingFee,
        string paymentMethodId,
        string paymentMethodName,
        int? installmentMonths,
        DateTime createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(contact.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(contact.Phone);
        ArgumentException.ThrowIfNullOrWhiteSpace(contact.Address);

        Id = Guid.NewGuid();
        Code = code;
        CustomerName = contact.Name;
        CustomerPhone = contact.Phone;
        CustomerEmail = contact.Email;
        Province = contact.Province;
        Ward = contact.Ward;
        Address = contact.Address;
        CustomerNote = contact.Note;
        foreach (var item in items)
        {
            item.AttachTo(Id);
            _items.Add(item);
        }
        if (_items.Count == 0)
        {
            throw new DomainException("Đơn hàng chưa có sản phẩm.");
        }

        ShippingMethodId = shippingMethodId;
        ShippingMethodName = shippingMethodName;
        ShippingFee = shippingFee;
        PaymentMethodId = paymentMethodId;
        PaymentMethodName = paymentMethodName;
        InstallmentMonths = installmentMonths;
        Subtotal = _items.Sum(i => i.LineTotal);
        Total = Subtotal + shippingFee;
        Status = OrderStatus.New;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        _events.Add(new OrderEvent(Id, createdAt, null, null, OrderStatus.New, null, null));
    }

    public string Code { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    // Admin đánh dấu khi đã nhận tiền (chuyển khoản, COD đã thu…)
    public bool IsPaid { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public string CustomerName { get; private set; } = null!;
    public string CustomerPhone { get; private set; } = null!;
    public string? CustomerEmail { get; private set; }
    public string? Province { get; private set; }
    public string? Ward { get; private set; }
    // Địa chỉ đầy đủ (số nhà, đường, phường, tỉnh)
    public string Address { get; private set; } = null!;
    public string? CustomerNote { get; private set; }

    public string ShippingMethodId { get; private set; } = null!;
    public string ShippingMethodName { get; private set; } = null!;
    public decimal ShippingFee { get; private set; }
    public string PaymentMethodId { get; private set; } = null!;
    public string PaymentMethodName { get; private set; } = null!;
    public int? InstallmentMonths { get; private set; }

    public decimal Subtotal { get; private set; }
    public decimal Total { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items;
    public IReadOnlyList<OrderEvent> Events => _events;

    // Admin xử lý đơn: đổi trạng thái (bắt buộc ghi chú), đánh dấu thu tiền, hoặc chỉ thêm ghi chú — mỗi lần một dòng lịch sử
    public void Update(OrderStatus status, bool isPaid, string? note, string actor, DateTime now)
    {
        if (!Enum.IsDefined(status))
        {
            throw new DomainException("Trạng thái đơn hàng không hợp lệ.");
        }
        note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (note?.Length > MaxNoteLength)
        {
            throw new DomainException($"Ghi chú tối đa {MaxNoteLength} ký tự.");
        }

        var statusChanged = status != Status;
        var paidChanged = isPaid != IsPaid;
        if (statusChanged && note is null)
        {
            throw new DomainException("Đổi trạng thái đơn cần nhập ghi chú (lý do hoặc việc đã làm).");
        }
        if (!statusChanged && !paidChanged && note is null)
        {
            throw new DomainException("Chưa có gì thay đổi để lưu.");
        }

        _events.Add(new OrderEvent(
            Id,
            now,
            actor,
            statusChanged ? Status : null,
            statusChanged ? status : null,
            paidChanged ? isPaid : null,
            note));
        Status = status;
        IsPaid = isPaid;
        UpdatedAt = now;
    }
}
