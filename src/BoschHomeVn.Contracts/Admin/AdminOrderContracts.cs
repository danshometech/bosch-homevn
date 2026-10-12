using System.ComponentModel.DataAnnotations;

namespace BoschHomeVn.Contracts.Admin;

// Status: New | Confirmed | Shipping | Completed | Cancelled
public sealed record AdminOrderSummaryResponse(
    string Code,
    DateTime CreatedAt,
    string Status,
    bool IsPaid,
    string CustomerName,
    string CustomerPhone,
    string PaymentMethodName,
    int ItemCount,
    decimal Total);

// Counts: số đơn theo trạng thái (theo từ khóa tìm, chưa lọc trạng thái)
public sealed record AdminOrderPageResponse(
    IReadOnlyList<AdminOrderSummaryResponse> Items,
    int Total,
    int Page,
    int PageSize,
    IReadOnlyDictionary<string, int> Counts);

public sealed record AdminOrderItemResponse(
    string ProductId,
    string Model,
    string Name,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

// Lịch sử xử lý: Actor null = khách đặt hàng; FromStatus / ToStatus / IsPaid null = không đổi
public sealed record AdminOrderEventResponse(
    DateTime CreatedAt,
    string? Actor,
    string? FromStatus,
    string? ToStatus,
    bool? IsPaid,
    string? Note);

// Events: mới nhất trước
public sealed record AdminOrderResponse(
    string Code,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Status,
    bool IsPaid,
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail,
    string? Province,
    string? Ward,
    string Address,
    string? CustomerNote,
    string ShippingMethodName,
    decimal ShippingFee,
    string PaymentMethodName,
    int? InstallmentMonths,
    decimal Subtotal,
    decimal Total,
    IReadOnlyList<AdminOrderItemResponse> Items,
    IReadOnlyList<AdminOrderEventResponse> Events);

// Note: bắt buộc khi đổi Status; ghi vào lịch sử đơn
public sealed record UpdateOrderRequest([StringLength(20)] string Status, bool IsPaid, [StringLength(1000)] string? Note);

public sealed record NewOrderCountResponse(int Count);
