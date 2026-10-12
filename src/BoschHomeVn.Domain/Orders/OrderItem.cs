using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Domain.Orders;

// Một dòng sản phẩm trong đơn: mã, tên, ảnh, đơn giá chụp lại lúc đặt (sản phẩm bị sửa / xóa thì đơn vẫn giữ nguyên)
public sealed class OrderItem
{
    public const int MaxQuantity = 99;

    private OrderItem() { } // EF Core

    public OrderItem(string productId, string model, string name, string? imageUrl, decimal unitPrice, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (quantity is < 1 or > MaxQuantity)
        {
            throw new DomainException($"Số lượng mỗi sản phẩm từ 1 đến {MaxQuantity}.");
        }

        ProductId = productId;
        Model = model;
        Name = name;
        ImageUrl = imageUrl;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Guid OrderId { get; private set; }
    public string ProductId { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;

    internal void AttachTo(Guid orderId) => OrderId = orderId;
}
