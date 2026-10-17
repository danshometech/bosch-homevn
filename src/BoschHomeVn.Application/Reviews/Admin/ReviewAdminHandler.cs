using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Common;
using BoschHomeVn.Domain.Reviews;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Reviews.Admin;

// Search: tên / nội dung (không dấu), số điện thoại (từ 3 chữ số), tên hoặc mã sản phẩm
public sealed record ReviewAdminQuery(ReviewStatus? Status, string? Search, string? ProductId, int? Stars, int Page, int PageSize);

public sealed record ReviewAdminItem(ProductReview Review, string ProductName, string ProductModel, string? ProductImageUrl);

// Counts: số đánh giá theo từng trạng thái (đã áp dụng Search / ProductId / Stars, chưa lọc Status)
public sealed record ReviewAdminPage(IReadOnlyList<ReviewAdminItem> Items, int Total, IReadOnlyDictionary<ReviewStatus, int> Counts);

// CreatedAt: ngày khách đánh giá (admin nhập lại đánh giá nhận qua điện thoại, Zalo…); null = bây giờ
public sealed record SaveReviewCommand(
    string ProductId,
    int Rating,
    string Content,
    string AuthorName,
    string? AuthorPhone,
    IReadOnlyList<string> Photos,
    bool IsVerifiedPurchase,
    ReviewStatus Status,
    string? Reply,
    DateTime? CreatedAt,
    int HelpfulCount);

public sealed class ReviewAdminHandler(IAppDbContext db)
{
    private sealed class Row
    {
        public required ProductReview R { get; init; }
        public required Product P { get; init; }
    }

    public async Task<ReviewAdminPage> ListAsync(ReviewAdminQuery q, CancellationToken cancellationToken)
    {
        var query = Filter(q);
        var counts = await query
            .GroupBy(x => x.R.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        if (q.Status is { } status)
        {
            query = query.Where(x => x.R.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.R.CreatedAt)
            .ThenByDescending(x => x.R.Id)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(x => new ReviewAdminItem(x.R, x.P.Name, x.P.Model, x.P.ImageUrl))
            .ToListAsync(cancellationToken);
        return new ReviewAdminPage(items, total, counts);
    }

    public Task<ReviewAdminItem?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        Rows()
            .Where(x => x.R.Id == id)
            .Select(x => new ReviewAdminItem(x.R, x.P.Name, x.P.Model, x.P.ImageUrl))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<int> CountPendingAsync(CancellationToken cancellationToken) =>
        db.ProductReviews.CountAsync(r => r.Status == ReviewStatus.Pending, cancellationToken);

    public async Task<Guid> CreateAsync(SaveReviewCommand c, string actor, CancellationToken cancellationToken)
    {
        await EnsureProductAsync(c.ProductId, cancellationToken);
        var now = DateTime.UtcNow;
        var review = new ProductReview(
            c.ProductId, c.Rating, c.Content, c.AuthorName, c.AuthorPhone, c.Photos, c.IsVerifiedPurchase, c.Status, actor, DateOf(c, now));
        review.SetReply(c.Reply, now);
        review.SetHelpfulCount(c.HelpfulCount);

        db.ProductReviews.Add(review);
        await db.SaveChangesAsync(cancellationToken);
        await db.RefreshRatingAsync(c.ProductId, cancellationToken);
        return review.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, SaveReviewCommand c, CancellationToken cancellationToken)
    {
        var review = await db.ProductReviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (review is null)
        {
            return false;
        }
        if (c.ProductId != review.ProductId)
        {
            throw new DomainException("Không đổi được sản phẩm của đánh giá — xóa rồi tạo đánh giá mới cho sản phẩm kia.");
        }

        var now = DateTime.UtcNow;
        review.Edit(c.Rating, c.Content, c.AuthorName, c.AuthorPhone, c.Photos, c.IsVerifiedPurchase, DateOf(c, review.CreatedAt));
        review.SetStatus(c.Status);
        review.SetReply(c.Reply, now);
        review.SetHelpfulCount(c.HelpfulCount);
        review.Touch(now);
        await db.SaveChangesAsync(cancellationToken);
        await db.RefreshRatingAsync(review.ProductId, cancellationToken);
        return true;
    }

    // Duyệt / ẩn nhanh từ danh sách
    public async Task<bool> SetStatusAsync(Guid id, ReviewStatus status, CancellationToken cancellationToken)
    {
        var review = await db.ProductReviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (review is null)
        {
            return false;
        }

        review.SetStatus(status);
        review.Touch(DateTime.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        await db.RefreshRatingAsync(review.ProductId, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var review = await db.ProductReviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (review is null)
        {
            return false;
        }

        db.ProductReviews.Remove(review);
        await db.SaveChangesAsync(cancellationToken);
        await db.RefreshRatingAsync(review.ProductId, cancellationToken);
        return true;
    }

    private static DateTime DateOf(SaveReviewCommand c, DateTime fallback)
    {
        if (c.CreatedAt is not { } at)
        {
            return fallback;
        }
        var utc = at.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(at, DateTimeKind.Utc) : at.ToUniversalTime();
        if (utc > DateTime.UtcNow.AddMinutes(5))
        {
            throw new DomainException("Ngày đánh giá không được ở tương lai.");
        }
        return utc;
    }

    private async Task EnsureProductAsync(string productId, CancellationToken cancellationToken)
    {
        if (!await db.Products.AnyAsync(p => p.Id == productId, cancellationToken))
        {
            throw new DomainException("Sản phẩm không tồn tại.");
        }
    }

    private IQueryable<Row> Rows() =>
        from r in db.ProductReviews.AsNoTracking()
        join p in db.Products on r.ProductId equals p.Id
        select new Row { R = r, P = p };

    private IQueryable<Row> Filter(ReviewAdminQuery q)
    {
        var query = Rows();
        if (q.ProductId is { } productId)
        {
            query = query.Where(x => x.R.ProductId == productId);
        }
        if (q.Stars is { } stars)
        {
            query = query.Where(x => x.R.Rating == stars);
        }
        if (string.IsNullOrWhiteSpace(q.Search))
        {
            return query;
        }

        var term = SearchText.Normalize(q.Search);
        var model = q.Search.Trim().ToUpperInvariant();
        var digits = new string(q.Search.Where(char.IsDigit).ToArray());
        return query.Where(x =>
            SearchText.Fold(x.R.AuthorName).Contains(term)
            || SearchText.Fold(x.R.Content).Contains(term)
            || SearchText.Fold(x.P.Name).Contains(term)
            || x.P.Model.Contains(model)
            || (digits.Length >= 3 && x.R.AuthorPhone != null && x.R.AuthorPhone.Contains(digits)));
    }
}
