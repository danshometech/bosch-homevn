using System.ComponentModel.DataAnnotations;

namespace BoschHomeVn.Contracts.Admin;

// Status: Pending | Approved | Hidden. CreatedBy: tài khoản admin đã nhập, null = khách gửi trên site
public sealed record AdminReviewResponse(
    Guid Id,
    string ProductId,
    string ProductName,
    string ProductModel,
    string? ProductImageUrl,
    int Rating,
    string Content,
    string AuthorName,
    string? AuthorPhone,
    IReadOnlyList<string> Photos,
    bool IsVerifiedPurchase,
    string Status,
    int HelpfulCount,
    string? Reply,
    DateTime? RepliedAt,
    string? CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// Counts: số đánh giá theo trạng thái (theo từ khóa / sản phẩm / số sao, chưa lọc trạng thái)
public sealed record AdminReviewPageResponse(
    IReadOnlyList<AdminReviewResponse> Items,
    int Total,
    int Page,
    int PageSize,
    IReadOnlyDictionary<string, int> Counts);

// CreatedAt: ngày khách đánh giá, null = bây giờ
public sealed record SaveReviewRequest(
    [StringLength(64)] string ProductId,
    [Range(1, 5)] int Rating,
    [StringLength(1000)] string Content,
    [StringLength(100)] string AuthorName,
    [StringLength(20)] string? AuthorPhone,
    IReadOnlyList<string>? Photos,
    bool IsVerifiedPurchase,
    [StringLength(20)] string Status,
    [StringLength(1000)] string? Reply,
    DateTime? CreatedAt,
    [Range(0, 1_000_000)] int HelpfulCount);

public sealed record UpdateReviewStatusRequest([StringLength(20)] string Status);

public sealed record PendingReviewCountResponse(int Count);
