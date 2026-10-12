using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BoschHomeVn.Api.Infrastructure;
using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.Orders.Admin;
using BoschHomeVn.Contracts.Admin;
using BoschHomeVn.Domain.Common;
using BoschHomeVn.Domain.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Policy = AdminAuth.Policy)]
public sealed class AdminOrdersController(OrderAdminHandler handler) : ControllerBase
{
    // from / to: ngày đặt (yyyy-MM-dd, giờ Việt Nam), tính cả hai đầu
    [HttpGet]
    public async Task<AdminOrderPageResponse> List(
        [FromQuery] string? status,
        [FromQuery] string? q,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery, Range(1, 100_000)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        OrderStatus? filter = string.IsNullOrEmpty(status) ? null : OrderMappings.ParseStatus(status);
        var result = await handler.ListAsync(new OrderListQuery(filter, q, from, to, page, pageSize), cancellationToken);
        return new AdminOrderPageResponse(
            [.. result.Items.Select(o => o.ToSummary())],
            result.Total,
            page,
            pageSize,
            result.Counts.ToDictionary(x => x.Key.ToString(), x => x.Value));
    }

    // File Excel mọi đơn đặt trong khoảng ngày (tối đa 31 ngày), mỗi dòng sản phẩm một hàng
    [HttpGet("export")]
    public async Task<FileContentResult> Export([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken)
    {
        if (from is not { } f || to is not { } t)
        {
            throw new DomainException("Chọn khoảng ngày cần xuất.");
        }

        var orders = await handler.ExportAsync(f, t, cancellationToken);
        return File(OrderExcel.Build(orders), OrderExcel.ContentType, $"danh-sach-don-hang-{Day(f)}-{Day(t)}.xlsx");
    }

    [HttpGet("new-count")]
    public async Task<NewOrderCountResponse> NewCount(CancellationToken cancellationToken) =>
        new(await handler.CountNewAsync(cancellationToken));

    [HttpGet("{code}")]
    public async Task<ActionResult<AdminOrderResponse>> Get(string code, CancellationToken cancellationToken)
    {
        var order = await handler.GetAsync(code, cancellationToken);
        return order is null ? NotFound() : order.ToAdminResponse();
    }

    // File Excel chi tiết một đơn: thông tin đơn, người nhận, giao hàng / thanh toán, sản phẩm, lịch sử xử lý
    [HttpGet("{code}/export")]
    public async Task<IActionResult> ExportOne(string code, CancellationToken cancellationToken)
    {
        var order = await handler.GetAsync(code, cancellationToken);
        return order is null ? NotFound() : File(OrderExcel.BuildDetail(order), OrderExcel.ContentType, $"chi-tiet-don-hang-{order.Code}.xlsx");
    }

    [HttpPut("{code}")]
    public async Task<ActionResult<AdminOrderResponse>> Update(string code, UpdateOrderRequest request, CancellationToken cancellationToken)
    {
        var actor = User.Identity?.Name ?? "admin";
        if (!await handler.UpdateAsync(code, OrderMappings.ParseStatus(request.Status), request.IsPaid, request.Note, actor, cancellationToken))
        {
            return NotFound();
        }
        return (await handler.GetAsync(code, cancellationToken))!.ToAdminResponse();
    }

    private static string Day(DateOnly day) => day.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
}
