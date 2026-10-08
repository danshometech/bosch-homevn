using BoschHomeVn.Api.Infrastructure;
using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.Catalog.Admin;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = AdminAuth.Policy)]
public sealed class AdminCategoriesController(CategoryAdminHandler handler) : ControllerBase
{
    [HttpGet("categories")]
    public async Task<IReadOnlyList<AdminCategoryResponse>> List(CancellationToken cancellationToken) =>
        (await handler.GetTreeAsync(cancellationToken)).ToResponse();

    [HttpPost("categories")]
    public async Task<CreatedResponse> CreateCategory(SaveCategoryRequest request, CancellationToken cancellationToken) =>
        new(await handler.CreateCategoryAsync(request.Name, request.ShortName, cancellationToken));

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(string id, SaveCategoryRequest request, CancellationToken cancellationToken) =>
        await handler.UpdateCategoryAsync(id, request.Name, request.ShortName, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(string id, CancellationToken cancellationToken) =>
        await handler.DeleteCategoryAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpPut("categories/order")]
    public async Task<IActionResult> ReorderCategories(ReorderRequest request, CancellationToken cancellationToken)
    {
        await handler.ReorderCategoriesAsync(request.Ids, cancellationToken);
        return NoContent();
    }

    [HttpPost("categories/{categoryId}/types")]
    public async Task<ActionResult<CreatedResponse>> CreateType(
        string categoryId, SaveProductTypeRequest request, CancellationToken cancellationToken)
    {
        var id = await handler.CreateTypeAsync(categoryId, request.Name, request.Group, cancellationToken);
        return id is null ? NotFound() : new CreatedResponse(id);
    }

    [HttpPut("categories/{categoryId}/types/order")]
    public async Task<IActionResult> ReorderTypes(string categoryId, ReorderRequest request, CancellationToken cancellationToken) =>
        await handler.ReorderTypesAsync(categoryId, request.Ids, cancellationToken) ? NoContent() : NotFound();

    [HttpPut("types/{id}")]
    public async Task<IActionResult> UpdateType(string id, SaveProductTypeRequest request, CancellationToken cancellationToken) =>
        await handler.UpdateTypeAsync(id, request.Name, request.Group, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("types/{id}")]
    public async Task<IActionResult> DeleteType(string id, CancellationToken cancellationToken) =>
        await handler.DeleteTypeAsync(id, cancellationToken) ? NoContent() : NotFound();
}
