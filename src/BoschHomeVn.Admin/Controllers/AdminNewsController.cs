using BoschHomeVn.Admin.Infrastructure;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

[ApiController]
[Route("api/admin/news")]
public sealed class AdminNewsController(AdminApiClient api) : ControllerBase
{
    private const string Base = "api/admin/news/";

    [HttpGet("categories")]
    public Task<ActionResult<IReadOnlyList<AdminNewsCategoryResponse>>> ListCategories(CancellationToken cancellationToken) =>
        api.GetAsync<IReadOnlyList<AdminNewsCategoryResponse>>(Base + "categories", cancellationToken);

    [HttpPost("categories")]
    public Task<ActionResult<CreatedResponse>> CreateCategory(SaveNewsCategoryRequest request, CancellationToken cancellationToken) =>
        api.PostAsync<CreatedResponse>(Base + "categories", request, cancellationToken);

    [HttpPut("categories/order")]
    public Task<IActionResult> ReorderCategories(ReorderRequest request, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Put, Base + "categories/order", request, cancellationToken);

    [HttpPut("categories/{id}")]
    public Task<IActionResult> UpdateCategory(string id, SaveNewsCategoryRequest request, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Put, Base + "categories/" + Uri.EscapeDataString(id), request, cancellationToken);

    [HttpDelete("categories/{id}")]
    public Task<IActionResult> DeleteCategory(string id, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Delete, Base + "categories/" + Uri.EscapeDataString(id), null, cancellationToken);

    [HttpGet("posts")]
    public Task<ActionResult<IReadOnlyList<AdminNewsPostListItem>>> ListPosts(CancellationToken cancellationToken) =>
        api.GetAsync<IReadOnlyList<AdminNewsPostListItem>>(Base + "posts", cancellationToken);

    [HttpGet("posts/{id:guid}")]
    public Task<ActionResult<AdminNewsPostResponse>> GetPost(Guid id, CancellationToken cancellationToken) =>
        api.GetAsync<AdminNewsPostResponse>(Base + "posts/" + id, cancellationToken);

    [HttpPost("posts")]
    public Task<ActionResult<AdminNewsPostResponse>> CreatePost(SaveNewsPostRequest request, CancellationToken cancellationToken) =>
        api.PostAsync<AdminNewsPostResponse>(Base + "posts", request, cancellationToken);

    [HttpPut("posts/{id:guid}")]
    public Task<ActionResult<AdminNewsPostResponse>> UpdatePost(Guid id, SaveNewsPostRequest request, CancellationToken cancellationToken) =>
        api.PutAsync<AdminNewsPostResponse>(Base + "posts/" + id, request, cancellationToken);

    [HttpDelete("posts/{id:guid}")]
    public Task<IActionResult> DeletePost(Guid id, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Delete, Base + "posts/" + id, null, cancellationToken);
}
