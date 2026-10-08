using BoschHomeVn.Admin.Infrastructure;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

[ApiController]
[Route("api/admin")]
public sealed class AdminCategoriesController(AdminApiClient api) : ControllerBase
{
    [HttpGet("categories")]
    public Task<ActionResult<IReadOnlyList<AdminCategoryResponse>>> List(CancellationToken cancellationToken) =>
        api.GetAsync<IReadOnlyList<AdminCategoryResponse>>("api/admin/categories", cancellationToken);

    [HttpPost("categories")]
    public Task<ActionResult<CreatedResponse>> CreateCategory(SaveCategoryRequest request, CancellationToken cancellationToken) =>
        api.PostAsync<CreatedResponse>("api/admin/categories", request, cancellationToken);

    [HttpPut("categories/{id}")]
    public Task<IActionResult> UpdateCategory(string id, SaveCategoryRequest request, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Put, "api/admin/categories/" + Uri.EscapeDataString(id), request, cancellationToken);

    [HttpDelete("categories/{id}")]
    public Task<IActionResult> DeleteCategory(string id, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Delete, "api/admin/categories/" + Uri.EscapeDataString(id), null, cancellationToken);

    [HttpPut("categories/order")]
    public Task<IActionResult> ReorderCategories(ReorderRequest request, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Put, "api/admin/categories/order", request, cancellationToken);

    [HttpPost("categories/{categoryId}/types")]
    public Task<ActionResult<CreatedResponse>> CreateType(string categoryId, SaveProductTypeRequest request, CancellationToken cancellationToken) =>
        api.PostAsync<CreatedResponse>($"api/admin/categories/{Uri.EscapeDataString(categoryId)}/types", request, cancellationToken);

    [HttpPut("categories/{categoryId}/types/order")]
    public Task<IActionResult> ReorderTypes(string categoryId, ReorderRequest request, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Put, $"api/admin/categories/{Uri.EscapeDataString(categoryId)}/types/order", request, cancellationToken);

    [HttpPut("types/{id}")]
    public Task<IActionResult> UpdateType(string id, SaveProductTypeRequest request, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Put, "api/admin/types/" + Uri.EscapeDataString(id), request, cancellationToken);

    [HttpDelete("types/{id}")]
    public Task<IActionResult> DeleteType(string id, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Delete, "api/admin/types/" + Uri.EscapeDataString(id), null, cancellationToken);
}
