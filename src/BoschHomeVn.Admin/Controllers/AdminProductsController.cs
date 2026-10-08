using BoschHomeVn.Admin.Infrastructure;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

[ApiController]
[Route("api/admin/products")]
public sealed class AdminProductsController(AdminApiClient api) : ControllerBase
{
    private const long MaxImageBytes = 5 * 1024 * 1024;
    private const long MaxVideoBytes = 50 * 1024 * 1024;

    [HttpGet]
    public Task<ActionResult<IReadOnlyList<AdminProductResponse>>> List(CancellationToken cancellationToken) =>
        api.GetAsync<IReadOnlyList<AdminProductResponse>>("api/admin/products", cancellationToken);

    [HttpGet("{id}")]
    public Task<ActionResult<AdminProductResponse>> Get(string id, CancellationToken cancellationToken) =>
        api.GetAsync<AdminProductResponse>(ProductPath(id), cancellationToken);

    [HttpPost]
    public async Task<ActionResult<AdminProductResponse>> Create(SaveProductRequest request, CancellationToken cancellationToken)
    {
        var result = await api.PostAsync<AdminProductResponse>("api/admin/products", request, cancellationToken);
        return result.Value is { } created ? CreatedAtAction(nameof(Get), new { id = created.Id }, created) : result;
    }

    [HttpPut("{id}")]
    public Task<ActionResult<AdminProductResponse>> Update(string id, SaveProductRequest request, CancellationToken cancellationToken) =>
        api.PutAsync<AdminProductResponse>(ProductPath(id), request, cancellationToken);

    [HttpDelete("{id}")]
    public Task<IActionResult> Delete(string id, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Delete, ProductPath(id), null, cancellationToken);

    // Tải ảnh lên (multipart, trường "file"), Api kiểm tra định dạng / dung lượng. folder: products | moments
    [HttpPost("/api/admin/media")]
    [RequestSizeLimit(MaxImageBytes + 64 * 1024)]
    public Task<ActionResult<MediaResponse>> Upload(IFormFile file, [FromForm] string? folder, CancellationToken cancellationToken) =>
        api.UploadAsync("api/admin/media", file, folder, cancellationToken);

    // Tải video lên: MP4 / WebM, tối đa 50 MB
    [HttpPost("/api/admin/media/video")]
    [RequestSizeLimit(MaxVideoBytes + 64 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxVideoBytes + 64 * 1024)]
    public Task<ActionResult<MediaResponse>> UploadVideo(IFormFile file, CancellationToken cancellationToken) =>
        api.UploadAsync("api/admin/media/video", file, null, cancellationToken);

    private static string ProductPath(string id) => "api/admin/products/" + Uri.EscapeDataString(id);
}
