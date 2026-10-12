using BoschHomeVn.Application.Orders;
using BoschHomeVn.Contracts.Admin;
using BoschHomeVn.Contracts.Orders;
using BoschHomeVn.Domain.Common;
using BoschHomeVn.Domain.Orders;

namespace BoschHomeVn.Api.Mappings;

internal static class OrderMappings
{
    public static PlaceOrderCommand ToCommand(this PlaceOrderRequest r)
    {
        var c = r.Customer ?? throw new DomainException("Thiếu thông tin người nhận.");
        return new PlaceOrderCommand(
            new OrderContact(c.Name, c.Phone, c.Email, c.Province, c.Ward, c.Address, c.Note),
            [.. (r.Items ?? []).Select(i => new PlaceOrderLine(i.Id, i.Qty, i.Price))],
            r.Shipping,
            r.Payment,
            r.Months,
            r.Total);
    }

    public static OrderStatus ParseStatus(string? value) =>
        Enum.TryParse<OrderStatus>(value, out var status) && Enum.IsDefined(status)
            ? status
            : throw new DomainException("Trạng thái đơn hàng phải là New, Confirmed, Shipping, Completed hoặc Cancelled.");

    public static AdminOrderSummaryResponse ToSummary(this Order o) =>
        new(o.Code, o.CreatedAt, o.Status.ToString(), o.IsPaid, o.CustomerName, o.CustomerPhone, o.PaymentMethodName, o.Items.Sum(i => i.Quantity), o.Total);

    public static AdminOrderResponse ToAdminResponse(this Order o) =>
        new(
            o.Code,
            o.CreatedAt,
            o.UpdatedAt,
            o.Status.ToString(),
            o.IsPaid,
            o.CustomerName,
            o.CustomerPhone,
            o.CustomerEmail,
            o.Province,
            o.Ward,
            o.Address,
            o.CustomerNote,
            o.ShippingMethodName,
            o.ShippingFee,
            o.PaymentMethodName,
            o.InstallmentMonths,
            o.Subtotal,
            o.Total,
            [.. o.Items.Select(i => new AdminOrderItemResponse(i.ProductId, i.Model, i.Name, i.ImageUrl, i.UnitPrice, i.Quantity, i.LineTotal))],
            [.. o.Events
                .OrderByDescending(e => e.CreatedAt)
                .ThenByDescending(e => e.Id)
                .Select(e => new AdminOrderEventResponse(e.CreatedAt, e.Actor, e.FromStatus?.ToString(), e.ToStatus?.ToString(), e.IsPaid, e.Note))]);
}
