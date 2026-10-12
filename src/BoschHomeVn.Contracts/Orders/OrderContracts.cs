using System.ComponentModel.DataAnnotations;

namespace BoschHomeVn.Contracts.Orders;

// Khách đặt hàng: Items[].Price = giá khách thấy lúc bấm đặt, Total = tổng khách thấy (kể cả phí giao) — server đối chiếu
public sealed record PlaceOrderRequest(
    OrderCustomerRequest Customer,
    IReadOnlyList<OrderItemRequest> Items,
    [StringLength(64)] string Shipping,
    [StringLength(64)] string Payment,
    [Range(1, 60)] int? Months,
    decimal Total);

public sealed record OrderCustomerRequest(
    [StringLength(100)] string Name,
    [StringLength(20)] string Phone,
    [StringLength(200)] string? Email,
    [StringLength(100)] string? Province,
    [StringLength(100)] string? Ward,
    [StringLength(500)] string Address,
    [StringLength(1000)] string? Note);

public sealed record OrderItemRequest([StringLength(64)] string Id, int Qty, decimal Price);

public sealed record PlaceOrderResponse(string Code, decimal Total);
