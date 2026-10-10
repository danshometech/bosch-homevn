using BoschHomeVn.Contracts.News;
using BoschHomeVn.WebStore.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Controllers;

[ApiController]
[Route("api/news")]
public sealed class NewsController(StoreApiClient api) : ControllerBase
{
    [HttpGet("categories")]
    public Task<ActionResult<IReadOnlyList<NewsCategoryResponse>>> GetCategories(CancellationToken cancellationToken) =>
        api.GetNewsCategoriesAsync(cancellationToken);

    [HttpGet("posts")]
    public Task<ActionResult<NewsPostPageResponse>> GetPosts(
        [FromQuery] string? category, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken) =>
        api.GetNewsPostsAsync(category, page, pageSize, cancellationToken);

    [HttpGet("posts/{slug}")]
    public Task<ActionResult<NewsPostResponse>> GetPost(string slug, CancellationToken cancellationToken) =>
        api.GetNewsPostAsync(slug, cancellationToken);
}
