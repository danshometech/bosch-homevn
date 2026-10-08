using BoschHomeVn.Api.Infrastructure;
using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.Abstractions.Media;
using BoschHomeVn.Application.Catalog.Admin;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/products")]
[Authorize(Policy = AdminAuth.Policy)]
public sealed class AdminProductsController(ProductAdminHandler handler) : ControllerBase
{
    private const long MaxImageBytes = 5 * 1024 * 1024;
    private const long MaxVideoBytes = 50 * 1024 * 1024;

    // Tất cả sản phẩm (kể cả chưa có giá / đang ẩn) — trang quản trị lọc, phân trang trên trình duyệt
    [HttpGet]
    public async Task<IReadOnlyList<AdminProductResponse>> List(CancellationToken cancellationToken) =>
        [.. (await handler.ListAsync(cancellationToken)).Select(p => p.ToAdminResponse())];

    [HttpGet("{id}")]
    public async Task<ActionResult<AdminProductResponse>> Get(string id, CancellationToken cancellationToken)
    {
        var product = await handler.GetAsync(id, cancellationToken);
        return product is null ? NotFound() : product.ToAdminResponse();
    }

    [HttpPost]
    public async Task<ActionResult<AdminProductResponse>> Create(SaveProductRequest request, CancellationToken cancellationToken)
    {
        var id = await handler.CreateAsync(request.ToCommand(), cancellationToken);
        var created = await handler.GetAsync(id, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, created!.ToAdminResponse());
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AdminProductResponse>> Update(string id, SaveProductRequest request, CancellationToken cancellationToken)
    {
        if (!await handler.UpdateAsync(id, request.ToCommand(), cancellationToken))
        {
            return NotFound();
        }
        return (await handler.GetAsync(id, cancellationToken))!.ToAdminResponse();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken) =>
        await handler.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    // Tải ảnh lên (multipart, trường "file"): JPG / PNG / WebP, tối đa 5 MB. folder: products | moments
    [HttpPost("/api/admin/media")]
    [RequestSizeLimit(MaxImageBytes + 64 * 1024)]
    public async Task<ActionResult<MediaResponse>> Upload(
        IFormFile file, [FromForm] string? folder, [FromServices] IMediaStorage storage, CancellationToken cancellationToken)
    {
        if (file.Length is 0 or > MaxImageBytes)
        {
            return Problem("Ảnh phải nhỏ hơn 5 MB.", statusCode: StatusCodes.Status400BadRequest);
        }

        await using var stream = file.OpenReadStream();
        return new MediaResponse(await storage.SaveImageAsync(stream, folder ?? "products", cancellationToken));
    }

    // Tải video lên (multipart, trường "file"): MP4 / WebM, tối đa 50 MB
    [HttpPost("/api/admin/media/video")]
    [RequestSizeLimit(MaxVideoBytes + 64 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxVideoBytes + 64 * 1024)]
    public async Task<ActionResult<MediaResponse>> UploadVideo(
        IFormFile file, [FromServices] IMediaStorage storage, CancellationToken cancellationToken)
    {
        if (file.Length is 0 or > MaxVideoBytes)
        {
            return Problem("Video phải nhỏ hơn 50 MB.", statusCode: StatusCodes.Status400BadRequest);
        }

        await using var stream = file.OpenReadStream();
        return new MediaResponse(await storage.SaveVideoAsync(stream, cancellationToken));
    }
}
