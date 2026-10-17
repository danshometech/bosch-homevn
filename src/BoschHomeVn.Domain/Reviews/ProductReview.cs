using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Domain.Reviews;

public enum ReviewStatus
{
    Pending,
    Approved,
    Hidden,
}

// Đánh giá sản phẩm: khách gửi trên site (chờ duyệt) hoặc admin nhập. Chỉ đánh giá Approved mới hiện trên site và được
// tính vào điểm trung bình của sản phẩm. AuthorPhone không bao giờ trả ra API công khai.
public sealed class ProductReview : Entity<Guid>
{
    public const int MaxContentLength = 1000;
    public const int MaxReplyLength = 1000;
    public const int MaxNameLength = 100;
    public const int MaxPhotos = 5;

    private ProductReview() { } // EF Core

    public ProductReview(
        string productId,
        int rating,
        string content,
        string authorName,
        string? authorPhone,
        IEnumerable<string> photos,
        bool isVerifiedPurchase,
        ReviewStatus status,
        string? createdBy,
        DateTime createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);

        Id = Guid.CreateVersion7();
        ProductId = productId;
        CreatedBy = createdBy;
        Edit(rating, content, authorName, authorPhone, photos, isVerifiedPurchase, createdAt);
        SetStatus(status);
        UpdatedAt = createdAt;
    }

    public string ProductId { get; private set; } = null!;
    public int Rating { get; private set; }
    public string Content { get; private set; } = null!;
    public string AuthorName { get; private set; } = null!;
    public string? AuthorPhone { get; private set; }
    public List<string> Photos { get; private set; } = [];
    public bool IsVerifiedPurchase { get; private set; }
    public ReviewStatus Status { get; private set; }
    public int HelpfulCount { get; private set; }
    public string? Reply { get; private set; }
    public DateTime? RepliedAt { get; private set; }
    // Tài khoản admin đã nhập đánh giá; null = khách gửi trên site
    public string? CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Edit(
        int rating, string content, string authorName, string? authorPhone, IEnumerable<string> photos, bool isVerifiedPurchase, DateTime createdAt)
    {
        if (rating is < 1 or > 5)
        {
            throw new DomainException("Số sao phải từ 1 đến 5.");
        }
        content = content?.Trim() ?? "";
        if (content.Length == 0)
        {
            throw new DomainException("Vui lòng nhập nội dung đánh giá.");
        }
        if (content.Length > MaxContentLength)
        {
            throw new DomainException($"Nội dung đánh giá tối đa {MaxContentLength} ký tự.");
        }
        authorName = authorName?.Trim() ?? "";
        if (authorName.Length == 0)
        {
            throw new DomainException("Vui lòng nhập họ tên.");
        }
        if (authorName.Length > MaxNameLength)
        {
            throw new DomainException($"Họ tên tối đa {MaxNameLength} ký tự.");
        }

        var list = photos.Select(u => u.Trim()).Where(u => u.Length > 0).Distinct().ToList();
        if (list.Count > MaxPhotos)
        {
            throw new DomainException($"Tối đa {MaxPhotos} ảnh.");
        }
        if (list.Any(u => u.Length > 500))
        {
            throw new DomainException("Đường dẫn ảnh tối đa 500 ký tự.");
        }

        Rating = rating;
        Content = content;
        AuthorName = authorName;
        AuthorPhone = string.IsNullOrWhiteSpace(authorPhone) ? null : authorPhone.Trim();
        Photos = list;
        IsVerifiedPurchase = isVerifiedPurchase;
        CreatedAt = createdAt;
    }

    public void SetStatus(ReviewStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new DomainException("Trạng thái đánh giá không hợp lệ.");
        }

        Status = status;
    }

    public void SetReply(string? reply, DateTime now)
    {
        reply = string.IsNullOrWhiteSpace(reply) ? null : reply.Trim();
        if (reply?.Length > MaxReplyLength)
        {
            throw new DomainException($"Phản hồi tối đa {MaxReplyLength} ký tự.");
        }
        if (reply == Reply)
        {
            return;
        }

        Reply = reply;
        RepliedAt = reply is null ? null : now;
    }

    public void Touch(DateTime now) => UpdatedAt = now;

    public void AddHelpful(int delta) => HelpfulCount = Math.Max(0, HelpfulCount + delta);

    public void SetHelpfulCount(int count)
    {
        if (count < 0)
        {
            throw new DomainException("Lượt hữu ích không được âm.");
        }

        HelpfulCount = count;
    }
}
