using BoschHomeVn.Contracts.Address;
using BoschHomeVn.Contracts.Catalog;
using BoschHomeVn.Contracts.Checkout;
using BoschHomeVn.Contracts.Home;
using BoschHomeVn.Contracts.News;
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

    public Task<HttpResponseMessage?> SendMediaAsync(Func<HttpRequestMessage> create, CancellationToken cancellationToken) =>
        ApiCall.SendMediaAsync(http, create, logger, cancellationToken);

    private Task<ActionResult<T>> GetAsync<T>(string path, CancellationToken cancellationToken) =>
        ApiCall.SendAsync<T>(http, () => new HttpRequestMessage(HttpMethod.Get, path), logger, cancellationToken);
}
