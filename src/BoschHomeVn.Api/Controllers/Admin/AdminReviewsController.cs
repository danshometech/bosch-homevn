using System.ComponentModel.DataAnnotations;
using BoschHomeVn.Api.Infrastructure;
using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.Reviews.Admin;
using BoschHomeVn.Contracts.Admin;
using BoschHomeVn.Domain.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Policy = AdminAuth.Policy)]
public sealed class AdminReviewsController(ReviewAdminHandler handler) : ControllerBase
{
    [HttpGet]
    public async Task<AdminReviewPageResponse> List(
        [FromQuery] string? status,
        [FromQuery] string? q,
        [FromQuery] string? product,
        [FromQuery, Range(1, 5)] int? stars,
        [FromQuery, Range(1, 100_000)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ReviewStatus? filter = string.IsNullOrEmpty(status) ? null : ReviewMappings.ParseStatus(status);
        var result = await handler.ListAsync(
            new ReviewAdminQuery(filter, q, string.IsNullOrEmpty(product) ? null : product, stars, page, pageSize), cancellationToken);
        return new AdminReviewPageResponse(
            [.. result.Items.Select(x => x.ToAdminResponse())],
            result.Total,
            page,
            pageSize,
            result.Counts.ToDictionary(x => x.Key.ToString(), x => x.Value));
    }

    [HttpGet("pending-count")]
    public async Task<PendingReviewCountResponse> PendingCount(CancellationToken cancellationToken) =>
        new(await handler.CountPendingAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminReviewResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var review = await handler.GetAsync(id, cancellationToken);
        return review is null ? NotFound() : review.ToAdminResponse();
    }

    [HttpPost]
    public async Task<ActionResult<AdminReviewResponse>> Create(SaveReviewRequest request, CancellationToken cancellationToken)
    {
        var id = await handler.CreateAsync(request.ToCommand(), User.Identity?.Name ?? "admin", cancellationToken);
        var created = await handler.GetAsync(id, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, created!.ToAdminResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminReviewResponse>> Update(Guid id, SaveReviewRequest request, CancellationToken cancellationToken)
    {
        if (!await handler.UpdateAsync(id, request.ToCommand(), cancellationToken))
        {
            return NotFound();
        }
        return (await handler.GetAsync(id, cancellationToken))!.ToAdminResponse();
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<AdminReviewResponse>> SetStatus(Guid id, UpdateReviewStatusRequest request, CancellationToken cancellationToken)
    {
        if (!await handler.SetStatusAsync(id, ReviewMappings.ParseStatus(request.Status), cancellationToken))
        {
            return NotFound();
        }
        return (await handler.GetAsync(id, cancellationToken))!.ToAdminResponse();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await handler.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
