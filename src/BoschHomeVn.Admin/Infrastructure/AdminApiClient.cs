using System.Net.Http.Headers;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Infrastructure;

// Gọi BoschHomeVn.Api (địa chỉ ở Api:BaseUrl) cho các controller của trang quản trị;
// cookie đăng nhập đi kèm qua ApiCookieRelayHandler. Kết quả / lỗi trả về dạng ActionResult (ApiCall).
public sealed class AdminApiClient(HttpClient http, ILogger<AdminApiClient> logger)
{
    public Task<ActionResult<T>> GetAsync<T>(string path, CancellationToken cancellationToken) =>
        ApiCall.SendAsync<T>(http, () => new HttpRequestMessage(HttpMethod.Get, path), logger, cancellationToken);

    public Task<ActionResult<T>> PostAsync<T>(string path, object body, CancellationToken cancellationToken) =>
        ApiCall.SendAsync<T>(http, () => new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) }, logger, cancellationToken);

    public Task<ActionResult<T>> PutAsync<T>(string path, object body, CancellationToken cancellationToken) =>
        ApiCall.SendAsync<T>(http, () => new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(body) }, logger, cancellationToken);

    // Endpoint trả 204 (không có body)
    public Task<IActionResult> SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken) =>
        ApiCall.SendAsync(
            http,
            () => new HttpRequestMessage(method, path) { Content = body is null ? null : JsonContent.Create(body) },
            logger,
            cancellationToken);

    // Chuyển file (ảnh / video) admin tải lên sang Api dạng multipart, trường "file" (+ "folder" nếu có)
    public async Task<ActionResult<MediaResponse>> UploadAsync(string path, IFormFile file, string? folder, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return await ApiCall.SendAsync<MediaResponse>(http, () =>
        {
            var part = new StreamContent(stream);
            if (MediaTypeHeaderValue.TryParse(file.ContentType, out var type))
            {
                part.Headers.ContentType = type;
            }

            var form = new MultipartFormDataContent { { part, "file", file.FileName } };
            if (!string.IsNullOrEmpty(folder))
            {
                form.Add(new StringContent(folder), "folder");
            }
            return new HttpRequestMessage(HttpMethod.Post, path) { Content = form };
        }, logger, cancellationToken);
    }

    public Task<HttpResponseMessage?> SendMediaAsync(Func<HttpRequestMessage> create, CancellationToken cancellationToken) =>
        ApiCall.SendMediaAsync(http, create, logger, cancellationToken);
}
