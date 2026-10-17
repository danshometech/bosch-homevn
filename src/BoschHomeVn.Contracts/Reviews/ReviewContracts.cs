namespace BoschHomeVn.Contracts.Reviews;

// Voted: trình duyệt đang xem đã bấm "Hữu ích" cho đánh giá này
public sealed record ReviewResponse(
    Guid Id,
    int Rating,
    string Content,
    string AuthorName,
    bool IsVerifiedPurchase,
    IReadOnlyList<string> Photos,
    int HelpfulCount,
    bool Voted,
    string? Reply,
    DateTime? RepliedAt,
    DateTime CreatedAt);

// Tính trên mọi đánh giá đang hiện của sản phẩm. Stars: số đánh giá theo mức sao, khóa "1".."5"
public sealed record ReviewSummaryResponse(decimal? Average, int Total, IReadOnlyDictionary<int, int> Stars, int WithPhotos, int Verified);

// Total: số đánh giá khớp bộ lọc đang chọn
public sealed record ReviewPageResponse(ReviewSummaryResponse Summary, IReadOnlyList<ReviewResponse> Items, int Page, int PageSize, int Total);

public sealed record SubmitReviewResponse(Guid Id);

public sealed record HelpfulResponse(int HelpfulCount, bool Voted);
