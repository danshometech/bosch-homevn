using BoschHomeVn.Api.Infrastructure;
using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.News.Admin;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/news")]
[Authorize(Policy = AdminAuth.Policy)]
public sealed class AdminNewsController(NewsAdminHandler handler) : ControllerBase
{
    [HttpGet("categories")]
    public async Task<IReadOnlyList<AdminNewsCategoryResponse>> ListCategories(CancellationToken cancellationToken) =>
        [.. (await handler.ListCategoriesAsync(cancellationToken)).Select(x => x.ToAdminResponse())];

    [HttpPost("categories")]
    public async Task<CreatedResponse> CreateCategory(SaveNewsCategoryRequest request, CancellationToken cancellationToken) =>
        new(await handler.CreateCategoryAsync(request.Name, request.Description, cancellationToken));

    [HttpPut("categories/order")]
    public async Task<IActionResult> ReorderCategories(ReorderRequest request, CancellationToken cancellationToken)
    {
        await handler.ReorderCategoriesAsync(request.Ids, cancellationToken);
        return NoContent();
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(string id, SaveNewsCategoryRequest request, CancellationToken cancellationToken) =>
        await handler.UpdateCategoryAsync(id, request.Name, request.Description, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(string id, CancellationToken cancellationToken) =>
        await handler.DeleteCategoryAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpGet("posts")]
    public async Task<IReadOnlyList<AdminNewsPostListItem>> ListPosts(CancellationToken cancellationToken) =>
        [.. (await handler.ListPostsAsync(cancellationToken)).Select(p => p.ToAdminResponse())];

    [HttpGet("posts/{id:guid}")]
    public async Task<ActionResult<AdminNewsPostResponse>> GetPost(Guid id, CancellationToken cancellationToken)
    {
        var post = await handler.GetPostAsync(id, cancellationToken);
        return post is null ? NotFound() : post.ToAdminResponse();
    }

    [HttpPost("posts")]
    public async Task<ActionResult<AdminNewsPostResponse>> CreatePost(SaveNewsPostRequest request, CancellationToken cancellationToken)
    {
        var id = await handler.CreatePostAsync(request.ToCommand(), cancellationToken);
        var created = await handler.GetPostAsync(id, cancellationToken);
        return CreatedAtAction(nameof(GetPost), new { id }, created!.ToAdminResponse());
    }

    [HttpPut("posts/{id:guid}")]
    public async Task<ActionResult<AdminNewsPostResponse>> UpdatePost(Guid id, SaveNewsPostRequest request, CancellationToken cancellationToken)
    {
        if (!await handler.UpdatePostAsync(id, request.ToCommand(), cancellationToken))
        {
            return NotFound();
        }
        return (await handler.GetPostAsync(id, cancellationToken))!.ToAdminResponse();
    }

    [HttpDelete("posts/{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken cancellationToken) =>
        await handler.DeletePostAsync(id, cancellationToken) ? NoContent() : NotFound();
}
