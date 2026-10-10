using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.News;
using BoschHomeVn.Contracts.News;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers;

[ApiController]
[Route("api/news")]
public sealed class NewsController(NewsHandler handler) : ControllerBase
{
    private const int MaxPageSize = 30;

    [HttpGet("categories")]
    public async Task<IReadOnlyList<NewsCategoryResponse>> GetCategories(CancellationToken cancellationToken) =>
        [.. (await handler.GetCategoriesAsync(cancellationToken)).Select(x => x.ToResponse())];

    [HttpGet("posts")]
    public async Task<ActionResult<NewsPostPageResponse>> GetPosts(
        [FromQuery] string? category, CancellationToken cancellationToken, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize is < 1 or > MaxPageSize)
        {
            return Problem($"page ≥ 1, pageSize trong khoảng 1–{MaxPageSize}.", statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await handler.GetPostsAsync(category, page, pageSize, cancellationToken);
        return new NewsPostPageResponse([.. result.Items.Select(x => x.ToResponse())], result.Total, page, pageSize);
    }

    [HttpGet("posts/{slug}")]
    public async Task<ActionResult<NewsPostResponse>> GetPost(string slug, CancellationToken cancellationToken)
    {
        var post = await handler.GetPostAsync(slug, cancellationToken);
        return post is null ? NotFound() : post.ToResponse();
    }
}
