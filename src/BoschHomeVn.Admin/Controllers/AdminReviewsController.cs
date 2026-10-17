using BoschHomeVn.Admin.Infrastructure;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

[ApiController]
[Route("api/admin/reviews")]
public sealed class AdminReviewsController(AdminApiClient api) : ControllerBase
{
    // Chuyển nguyên query string (status, q, product, stars, page, pageSize) — Api kiểm tra giá trị
    [HttpGet]
    public Task<ActionResult<AdminReviewPageResponse>> List(CancellationToken cancellationToken) =>
        api.GetAsync<AdminReviewPageResponse>("api/admin/reviews" + Request.QueryString, cancellationToken);

    [HttpGet("pending-count")]
    public Task<ActionResult<PendingReviewCountResponse>> PendingCount(CancellationToken cancellationToken) =>
        api.GetAsync<PendingReviewCountResponse>("api/admin/reviews/pending-count", cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<ActionResult<AdminReviewResponse>> Get(Guid id, CancellationToken cancellationToken) =>
        api.GetAsync<AdminReviewResponse>($"api/admin/reviews/{id}", cancellationToken);

    [HttpPost]
    public Task<ActionResult<AdminReviewResponse>> Create(SaveReviewRequest request, CancellationToken cancellationToken) =>
        api.PostAsync<AdminReviewResponse>("api/admin/reviews", request, cancellationToken);

    [HttpPut("{id:guid}")]
    public Task<ActionResult<AdminReviewResponse>> Update(Guid id, SaveReviewRequest request, CancellationToken cancellationToken) =>
        api.PutAsync<AdminReviewResponse>($"api/admin/reviews/{id}", request, cancellationToken);

    [HttpPut("{id:guid}/status")]
    public Task<ActionResult<AdminReviewResponse>> SetStatus(Guid id, UpdateReviewStatusRequest request, CancellationToken cancellationToken) =>
        api.PutAsync<AdminReviewResponse>($"api/admin/reviews/{id}/status", request, cancellationToken);

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        api.SendAsync(HttpMethod.Delete, $"api/admin/reviews/{id}", null, cancellationToken);
}
