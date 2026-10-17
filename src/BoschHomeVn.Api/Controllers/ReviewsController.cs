using System.Text.RegularExpressions;
using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.Reviews;
using BoschHomeVn.Contracts.Reviews;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers;

[ApiController]
[Route("api")]
public sealed partial class ReviewsController(ReviewsHandler handler) : ControllerBase
{
    private const string VisitorHeader = "X-Visitor-Id";
    private const int MaxPageSize = 20;
    private const long MaxPhotoBytes = 5 * 1024 * 1024;
    private const long MaxRequestBytes = 5 * MaxPhotoBytes + 64 * 1024;

    // stars: 1–5 · photos / verified: chỉ đánh giá có ảnh / đã mua hàng · sort: new | helpful | high | low
    [HttpGet("products/{id}/reviews")]
    public async Task<ActionResult<ReviewPageResponse>> List(
        string id,
        [FromQuery] int? stars,
        [FromQuery] bool photos,
        [FromQuery] bool verified,
        [FromQuery] string? sort,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5)
    {
        if (page < 1 || pageSize is < 1 or > MaxPageSize || stars is < 1 or > 5)
        {
            return Problem($"page ≥ 1, pageSize 1–{MaxPageSize}, stars 1–5.", statusCode: StatusCodes.Status400BadRequest);
        }
        var order = sort switch
        {
            null or "" or "new" => ReviewSort.Newest,
            "helpful" => ReviewSort.Helpful,
            "high" => ReviewSort.Highest,
            "low" => ReviewSort.Lowest,
            _ => (ReviewSort?)null,
        };
        if (order is null)
        {
            return Problem("sort phải là new, helpful, high hoặc low.", statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await handler.GetAsync(
            new ReviewListQuery(id, stars, photos, verified, order.Value, page, pageSize, VisitorId()), cancellationToken);
        if (result is null)
        {
            return NotFound();
        }
        return new ReviewPageResponse(
            result.Summary.ToResponse(), [.. result.Items.Select(r => r.ToResponse(result.Voted.Contains(r.Id)))], page, pageSize, result.Total);
    }

    // Multipart: rating, content, name, phone + tối đa 5 ảnh ở trường "photos"
    [HttpPost("products/{id}/reviews")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<ActionResult<SubmitReviewResponse>> Submit(
        string id,
        [FromForm] int rating,
        [FromForm] string? content,
        [FromForm] string? name,
        [FromForm] string? phone,
        [FromForm] IFormFileCollection? photos,
        CancellationToken cancellationToken)
    {
        var files = photos ?? new FormFileCollection();
        if (files.Any(f => f.Length is 0 or > MaxPhotoBytes))
        {
            return Problem("Mỗi ảnh phải nhỏ hơn 5 MB.", statusCode: StatusCodes.Status400BadRequest);
        }

        var streams = files.Select(f => f.OpenReadStream()).ToList();
        try
        {
            var reviewId = await handler.SubmitAsync(new SubmitReviewCommand(id, rating, content, name, phone, streams), cancellationToken);
            return new SubmitReviewResponse(reviewId);
        }
        finally
        {
            streams.ForEach(s => s.Dispose());
        }
    }

    [HttpPost("reviews/{id:guid}/helpful")]
    public Task<ActionResult<HelpfulResponse>> Helpful(Guid id, CancellationToken cancellationToken) =>
        HelpfulAsync(id, true, cancellationToken);

    [HttpDelete("reviews/{id:guid}/helpful")]
    public Task<ActionResult<HelpfulResponse>> Unhelpful(Guid id, CancellationToken cancellationToken) =>
        HelpfulAsync(id, false, cancellationToken);

    private async Task<ActionResult<HelpfulResponse>> HelpfulAsync(Guid id, bool on, CancellationToken cancellationToken)
    {
        if (VisitorId() is not { } visitorId)
        {
            return Problem($"Thiếu header {VisitorHeader}.", statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await handler.SetHelpfulAsync(id, visitorId, on, cancellationToken);
        return result is null ? NotFound() : new HelpfulResponse(result.HelpfulCount, result.Voted);
    }

    // Mã trình duyệt WebStore lấy từ cookie gửi kèm; sai định dạng coi như không có
    private string? VisitorId()
    {
        var value = Request.Headers[VisitorHeader].ToString();
        return VisitorIdFormat().IsMatch(value) ? value : null;
    }

    [GeneratedRegex("^[a-f0-9]{32}$")]
    private static partial Regex VisitorIdFormat();
}
