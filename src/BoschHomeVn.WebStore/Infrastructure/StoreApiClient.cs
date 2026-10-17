using System.Net.Http.Headers;
using BoschHomeVn.Contracts.Address;
using BoschHomeVn.Contracts.Catalog;
using BoschHomeVn.Contracts.Checkout;
using BoschHomeVn.Contracts.Home;
using BoschHomeVn.Contracts.News;
using BoschHomeVn.Contracts.Orders;
using BoschHomeVn.Contracts.Reviews;
using BoschHomeVn.Contracts.Settings;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Infrastructure;

public sealed class StoreApiClient(HttpClient http, ILogger<StoreApiClient> logger)
{
    public Task<ActionResult<MenuResponse>> GetMenuAsync(CancellationToken cancellationToken) =>
        GetAsync<MenuResponse>("api/menu", cancellationToken);

    public Task<ActionResult<HomePageResponse>> GetHomeAsync(CancellationToken cancellationToken) =>
        GetAsync<HomePageResponse>("api/home", cancellationToken);

    public Task<ActionResult<IReadOnlyList<ProductResponse>>> GetProductsAsync(
        string? category, string? q, bool flash, string? ids, int? take, CancellationToken cancellationToken)
    {
        var query = QueryString.Create(new Dictionary<string, string?>
        {
            ["category"] = category,
            ["q"] = q,
            ["flash"] = flash ? "true" : null,
            ["ids"] = ids,
            ["take"] = take?.ToString(),
        }.Where(p => !string.IsNullOrEmpty(p.Value)));
        return GetAsync<IReadOnlyList<ProductResponse>>("api/products" + query, cancellationToken);
    }

    // Chuyển nguyên query string (category, q, type…, page…) — Api kiểm tra giá trị
    public Task<ActionResult<ProductListingResponse>> GetProductListingAsync(QueryString query, CancellationToken cancellationToken) =>
        GetAsync<ProductListingResponse>("api/products/listing" + query, cancellationToken);

    public Task<ActionResult<ProductResponse>> GetProductAsync(string id, CancellationToken cancellationToken) =>
        GetAsync<ProductResponse>("api/products/" + Uri.EscapeDataString(id), cancellationToken);

    public Task<ActionResult<ProductArticleResponse>> GetProductArticleAsync(string id, CancellationToken cancellationToken) =>
        GetAsync<ProductArticleResponse>("api/products/" + Uri.EscapeDataString(id) + "/article", cancellationToken);

    public Task<ActionResult<IReadOnlyList<NewsCategoryResponse>>> GetNewsCategoriesAsync(CancellationToken cancellationToken) =>
        GetAsync<IReadOnlyList<NewsCategoryResponse>>("api/news/categories", cancellationToken);

    public Task<ActionResult<NewsPostPageResponse>> GetNewsPostsAsync(string? category, int? page, int? pageSize, CancellationToken cancellationToken)
    {
        var query = QueryString.Create(new Dictionary<string, string?>
        {
            ["category"] = category,
            ["page"] = page?.ToString(),
            ["pageSize"] = pageSize?.ToString(),
        }.Where(p => !string.IsNullOrEmpty(p.Value)));
        return GetAsync<NewsPostPageResponse>("api/news/posts" + query, cancellationToken);
    }

    public Task<ActionResult<NewsPostResponse>> GetNewsPostAsync(string slug, CancellationToken cancellationToken) =>
        GetAsync<NewsPostResponse>("api/news/posts/" + Uri.EscapeDataString(slug), cancellationToken);

    public Task<ActionResult<ContactSettingsResponse>> GetContactSettingsAsync(CancellationToken cancellationToken) =>
        GetAsync<ContactSettingsResponse>("api/settings/contact", cancellationToken);

    public Task<ActionResult<CheckoutOptionsResponse>> GetCheckoutOptionsAsync(CancellationToken cancellationToken) =>
        GetAsync<CheckoutOptionsResponse>("api/checkout/options", cancellationToken);

    public Task<ActionResult<IReadOnlyList<AdministrativeUnitResponse>>> GetProvincesAsync(CancellationToken cancellationToken) =>
        GetAsync<IReadOnlyList<AdministrativeUnitResponse>>("api/address/provinces", cancellationToken);

    public Task<ActionResult<IReadOnlyList<AdministrativeUnitResponse>>> GetWardsAsync(int provinceCode, CancellationToken cancellationToken) =>
        GetAsync<IReadOnlyList<AdministrativeUnitResponse>>($"api/address/provinces/{provinceCode}/wards", cancellationToken);

    // POST không tự gửi lại khi lỗi mạng (ApiCall chỉ thử lại GET) — tránh tạo trùng đơn
    public Task<ActionResult<PlaceOrderResponse>> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken cancellationToken) =>
        ApiCall.SendAsync<PlaceOrderResponse>(
            http, () => new HttpRequestMessage(HttpMethod.Post, "api/orders") { Content = JsonContent.Create(request) }, logger, cancellationToken);

    // visitorId: mã trình duyệt (cookie) để Api biết đánh giá nào trình duyệt này đã bấm "Hữu ích"
    public Task<ActionResult<ReviewPageResponse>> GetReviewsAsync(
        string productId, QueryString query, string? visitorId, CancellationToken cancellationToken) =>
        ApiCall.SendAsync<ReviewPageResponse>(http, () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/products/" + Uri.EscapeDataString(productId) + "/reviews" + query);
            if (visitorId is not null)
            {
                request.Headers.Add(VisitorHeader, visitorId);
            }
            return request;
        }, logger, cancellationToken);

    // Chuyển nguyên form khách gửi (các trường + ảnh) sang Api dạng multipart
    public async Task<ActionResult<SubmitReviewResponse>> SubmitReviewAsync(string productId, IFormCollection form, CancellationToken cancellationToken)
    {
        var streams = form.Files.Select(f => f.OpenReadStream()).ToList();
        try
        {
            return await ApiCall.SendAsync<SubmitReviewResponse>(http, () =>
            {
                var content = new MultipartFormDataContent();
                foreach (var name in new[] { "rating", "content", "name", "phone" })
                {
                    content.Add(new StringContent(form[name].ToString()), name);
                }
                for (var i = 0; i < form.Files.Count; i++)
                {
                    var file = form.Files[i];
                    var part = new StreamContent(streams[i]);
                    if (MediaTypeHeaderValue.TryParse(file.ContentType, out var type))
                    {
                        part.Headers.ContentType = type;
                    }
                    content.Add(part, "photos", file.FileName);
                }
                return new HttpRequestMessage(HttpMethod.Post, "api/products/" + Uri.EscapeDataString(productId) + "/reviews") { Content = content };
            }, logger, cancellationToken);
        }
        finally
        {
            streams.ForEach(s => s.Dispose());
        }
    }

    public Task<ActionResult<HelpfulResponse>> HelpfulAsync(Guid reviewId, bool add, string visitorId, CancellationToken cancellationToken) =>
        ApiCall.SendAsync<HelpfulResponse>(http, () =>
        {
            var request = new HttpRequestMessage(add ? HttpMethod.Post : HttpMethod.Delete, $"api/reviews/{reviewId}/helpful");
            request.Headers.Add(VisitorHeader, visitorId);
            return request;
        }, logger, cancellationToken);

    private const string VisitorHeader = "X-Visitor-Id";

    public Task<HttpResponseMessage?> SendMediaAsync(Func<HttpRequestMessage> create, CancellationToken cancellationToken) =>
        ApiCall.SendMediaAsync(http, create, logger, cancellationToken);

    private Task<ActionResult<T>> GetAsync<T>(string path, CancellationToken cancellationToken) =>
        ApiCall.SendAsync<T>(http, () => new HttpRequestMessage(HttpMethod.Get, path), logger, cancellationToken);
}
