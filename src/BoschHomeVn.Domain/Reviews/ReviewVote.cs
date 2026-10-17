namespace BoschHomeVn.Domain.Reviews;

// Một lượt bấm "Hữu ích": VisitorId = mã ngẫu nhiên WebStore cấp cho mỗi trình duyệt (cookie), không phải thông tin cá nhân.
// Mỗi trình duyệt bấm một đánh giá một lần
public sealed class ReviewVote
{
    public const int MaxVisitorIdLength = 64;

    private ReviewVote() { } // EF Core

    public ReviewVote(Guid reviewId, string visitorId, DateTime createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(visitorId);

        ReviewId = reviewId;
        VisitorId = visitorId;
        CreatedAt = createdAt;
    }

    public Guid ReviewId { get; private set; }
    public string VisitorId { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
}
