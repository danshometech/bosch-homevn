using BoschHomeVn.Admin.Infrastructure;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

[ApiController]
[Route("api/admin/orders")]
public sealed class AdminOrdersController(AdminApiClient api) : ControllerBase
{
    // Chuyển nguyên query string (status, q, from, to, page, pageSize) — Api kiểm tra giá trị
    [HttpGet]
    public Task<ActionResult<AdminOrderPageResponse>> List(CancellationToken cancellationToken) =>
        api.GetAsync<AdminOrderPageResponse>("api/admin/orders" + Request.QueryString, cancellationToken);

    // File Excel các đơn trong khoảng ngày (from, to)
    [HttpGet("export")]
    public Task<IActionResult> Export(CancellationToken cancellationToken) =>
        FileAsync("api/admin/orders/export" + Request.QueryString, cancellationToken);

    // File Excel chi tiết một đơn
    [HttpGet("{code}/export")]
    public Task<IActionResult> ExportOne(string code, CancellationToken cancellationToken) =>
        FileAsync("api/admin/orders/" + Uri.EscapeDataString(code) + "/export", cancellationToken);

    [HttpGet("new-count")]
    public Task<ActionResult<NewOrderCountResponse>> NewCount(CancellationToken cancellationToken) =>
        api.GetAsync<NewOrderCountResponse>("api/admin/orders/new-count", cancellationToken);

    [HttpGet("{code}")]
    public Task<ActionResult<AdminOrderResponse>> Get(string code, CancellationToken cancellationToken) =>
        api.GetAsync<AdminOrderResponse>("api/admin/orders/" + Uri.EscapeDataString(code), cancellationToken);

    [HttpPut("{code}")]
    public Task<ActionResult<AdminOrderResponse>> Update(string code, UpdateOrderRequest request, CancellationToken cancellationToken) =>
        api.PutAsync<AdminOrderResponse>("api/admin/orders/" + Uri.EscapeDataString(code), request, cancellationToken);

    // File từ Api chuyển nguyên về trình duyệt (kèm loại file, tên file); lỗi thì trả lại ProblemDetails / mã lỗi của Api
    private async Task<IActionResult> FileAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await api.SendMediaAsync(() => new HttpRequestMessage(HttpMethod.Get, path), cancellationToken);
        using var sent = response?.RequestMessage;
        if (response is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway);
        }
        if (!response.IsSuccessStatusCode)
        {
            return response.Content.Headers.ContentType?.MediaType?.Contains("json") == true
                ? new ContentResult
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync(cancellationToken),
                    ContentType = "application/problem+json",
                }
                : StatusCode((int)response.StatusCode);
        }

        var disposition = response.Content.Headers.ContentDisposition;
        var name = disposition?.FileNameStar ?? disposition?.FileName?.Trim('"') ?? "don-hang-export.xlsx";
        var type = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        return File(await response.Content.ReadAsByteArrayAsync(cancellationToken), type, name);
    }
}
