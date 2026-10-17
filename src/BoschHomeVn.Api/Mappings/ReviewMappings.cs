using BoschHomeVn.Application.Reviews;
using BoschHomeVn.Application.Reviews.Admin;
using BoschHomeVn.Contracts.Admin;
using BoschHomeVn.Contracts.Reviews;
using BoschHomeVn.Domain.Common;
using BoschHomeVn.Domain.Reviews;

namespace BoschHomeVn.Api.Mappings;

internal static class ReviewMappings
{
    public static ReviewResponse ToResponse(this ProductReview r, bool voted) =>
        new(r.Id, r.Rating, r.Content, r.AuthorName, r.IsVerifiedPurchase, r.Photos, r.HelpfulCount, voted, r.Reply, r.RepliedAt, r.CreatedAt);

    public static ReviewSummaryResponse ToResponse(this ReviewSummary s) =>
        new(s.Average, s.Total, s.Stars, s.WithPhotos, s.Verified);

    public static AdminReviewResponse ToAdminResponse(this ReviewAdminItem x)
    {
        var r = x.Review;
        return new(
            r.Id,
            r.ProductId,
            x.ProductName,
            x.ProductModel,
            x.ProductImageUrl,
            r.Rating,
            r.Content,
            r.AuthorName,
            r.AuthorPhone,
            r.Photos,
            r.IsVerifiedPurchase,
            r.Status.ToString(),
            r.HelpfulCount,
            r.Reply,
            r.RepliedAt,
            r.CreatedBy,
            r.CreatedAt,
            r.UpdatedAt);
    }

    public static SaveReviewCommand ToCommand(this SaveReviewRequest r) =>
        new(
            r.ProductId,
            r.Rating,
            r.Content,
            r.AuthorName,
            r.AuthorPhone,
            r.Photos ?? [],
            r.IsVerifiedPurchase,
            ParseStatus(r.Status),
            r.Reply,
            r.CreatedAt,
            r.HelpfulCount);

    public static ReviewStatus ParseStatus(string? value) =>
        Enum.TryParse<ReviewStatus>(value, out var status) && Enum.IsDefined(status)
            ? status
            : throw new DomainException("Trạng thái đánh giá phải là Pending, Approved hoặc Hidden.");
}
