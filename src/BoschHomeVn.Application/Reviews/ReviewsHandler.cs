using System.Text.RegularExpressions;
using BoschHomeVn.Application.Abstractions.Media;
using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Common;
using BoschHomeVn.Domain.Orders;
using BoschHomeVn.Domain.Reviews;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Reviews;

public enum ReviewSort { Newest, Helpful, Highest, Lowest }

// Stars: chỉ đánh giá đúng số sao này · WithPhotos: chỉ đánh giá có ảnh · VerifiedOnly: chỉ đánh giá "Đã mua hàng" ·
// VisitorId: mã trình duyệt đang xem (để biết đánh giá nào đã bấm "Hữu ích"), null = chưa có
public sealed record ReviewListQuery(
    string ProductId, int? Stars, bool WithPhotos, bool VerifiedOnly, ReviewSort Sort, int Page, int PageSize, string? VisitorId);

// Tính trên mọi đánh giá đang hiện của sản phẩm, không theo bộ lọc. Stars: số đánh giá theo từng mức 1–5 sao
public sealed record ReviewSummary(decimal? Average, int Total, IReadOnlyDictionary<int, int> Stars, int WithPhotos, int Verified);

// Total: số đánh giá khớp bộ lọc · Voted: các đánh giá trong trang trình duyệt này đã bấm "Hữu ích"
public sealed record ReviewPage(ReviewSummary Summary, IReadOnlyList<ProductReview> Items, int Total, IReadOnlySet<Guid> Voted);

public sealed record HelpfulResult(int HelpfulCount, bool Voted);

public sealed record SubmitReviewCommand(
    string ProductId, int Rating, string? Content, string? Name, string? Phone, IReadOnlyList<Stream> Photos);

// Khách xem / gửi đánh giá trên site. Đánh giá khách gửi chờ admin duyệt; số điện thoại trùng một đơn đã xác nhận có
// sản phẩm này thì gắn nhãn "Đã mua hàng". Mỗi số điện thoại đánh giá một sản phẩm một lần.
public sealed partial class ReviewsHandler(IAppDbContext db, IMediaStorage media)
{
    public const int MinContentLength = 20;

    public async Task<ReviewPage?> GetAsync(ReviewListQuery q, CancellationToken cancellationToken)
    {
        if (!await db.Products.AnyAsync(p => p.Id == q.ProductId && p.Price != null && p.IsPublished, cancellationToken))
        {
            return null;
        }

        var approved = db.ProductReviews.AsNoTracking().Where(r => r.ProductId == q.ProductId && r.Status == ReviewStatus.Approved);
        var groups = await approved
            .GroupBy(r => r.Rating)
            .Select(g => new
            {
                Stars = g.Key,
                Count = g.Count(),
                WithPhotos = g.Count(r => r.Photos.Count > 0),
                Verified = g.Count(r => r.IsVerifiedPurchase),
            })
            .ToListAsync(cancellationToken);
        var total = groups.Sum(g => g.Count);
        var summary = new ReviewSummary(
            total == 0 ? null : Math.Round((decimal)groups.Sum(g => g.Stars * g.Count) / total, 1),
            total,
            Enumerable.Range(1, 5).ToDictionary(s => s, s => groups.FirstOrDefault(g => g.Stars == s)?.Count ?? 0),
            groups.Sum(g => g.WithPhotos),
            groups.Sum(g => g.Verified));

        var query = approved;
        if (q.Stars is { } stars)
        {
            query = query.Where(r => r.Rating == stars);
        }
        if (q.WithPhotos)
        {
            query = query.Where(r => r.Photos.Count > 0);
        }
        if (q.VerifiedOnly)
        {
            query = query.Where(r => r.IsVerifiedPurchase);
        }

        var filtered = await query.CountAsync(cancellationToken);
        var sorted = q.Sort switch
        {
            ReviewSort.Helpful => query.OrderByDescending(r => r.HelpfulCount).ThenByDescending(r => r.CreatedAt),
            ReviewSort.Highest => query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
            ReviewSort.Lowest => query.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt),
        };
        var items = await sorted
            .ThenByDescending(r => r.Id)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(cancellationToken);

        var ids = items.Select(r => r.Id).ToList();
        var voted = q.VisitorId is null || ids.Count == 0
            ? []
            : await db.ReviewVotes
                .Where(v => v.VisitorId == q.VisitorId && ids.Contains(v.ReviewId))
                .Select(v => v.ReviewId)
                .ToListAsync(cancellationToken);
        return new ReviewPage(summary, items, filtered, voted.ToHashSet());
    }

    public async Task<Guid> SubmitAsync(SubmitReviewCommand c, CancellationToken cancellationToken)
    {
        var content = c.Content?.Trim() ?? "";
        if (content.Length < MinContentLength)
        {
            throw new DomainException($"Nội dung đánh giá tối thiểu {MinContentLength} ký tự.");
        }
        var phone = PhoneSeparators().Replace(c.Phone ?? "", "");
        if (!Phone().IsMatch(phone))
        {
            throw new DomainException("Số điện thoại gồm 10 số, bắt đầu bằng 0.");
        }
        if (c.Photos.Count > ProductReview.MaxPhotos)
        {
            throw new DomainException($"Tối đa {ProductReview.MaxPhotos} ảnh.");
        }
        if (!await db.Products.AnyAsync(p => p.Id == c.ProductId && p.Price != null && p.IsPublished, cancellationToken))
        {
            throw new DomainException("Sản phẩm không còn bán trên site.");
        }
        if (await db.ProductReviews.AnyAsync(r => r.ProductId == c.ProductId && r.AuthorPhone == phone, cancellationToken))
        {
            throw new DomainException("Số điện thoại này đã đánh giá sản phẩm, cảm ơn bạn!");
        }

        var verified = await db.Orders.AnyAsync(
            o => o.CustomerPhone == phone
                && o.Status != OrderStatus.New
                && o.Status != OrderStatus.Cancelled
                && o.Items.Any(i => i.ProductId == c.ProductId),
            cancellationToken);
        var now = DateTime.UtcNow;
        // Kiểm tra trước để không lưu ảnh của một đánh giá không hợp lệ
        var review = new ProductReview(c.ProductId, c.Rating, content, c.Name ?? "", phone, [], verified, ReviewStatus.Pending, null, now);

        var photos = new List<string>();
        foreach (var photo in c.Photos)
        {
            photos.Add(await media.SaveImageAsync(photo, "reviews", cancellationToken));
        }
        review.Edit(review.Rating, review.Content, review.AuthorName, review.AuthorPhone, photos, verified, now);

        db.ProductReviews.Add(review);
        await db.SaveChangesAsync(cancellationToken);
        return review.Id;
    }

    // on: khách bấm "Hữu ích" / bấm lại để bỏ. Mỗi trình duyệt (visitorId) tính một lượt; bấm trùng thì không đổi số.
    // null = không có đánh giá đang hiện
    public async Task<HelpfulResult?> SetHelpfulAsync(Guid id, string visitorId, bool on, CancellationToken cancellationToken)
    {
        var review = await db.ProductReviews.FirstOrDefaultAsync(r => r.Id == id && r.Status == ReviewStatus.Approved, cancellationToken);
        if (review is null)
        {
            return null;
        }

        var vote = await db.ReviewVotes.FirstOrDefaultAsync(v => v.ReviewId == id && v.VisitorId == visitorId, cancellationToken);
        if (on == vote is not null)
        {
            return new HelpfulResult(review.HelpfulCount, on);
        }

        if (on)
        {
            db.ReviewVotes.Add(new ReviewVote(id, visitorId, DateTime.UtcNow));
            review.AddHelpful(1);
        }
        else
        {
            db.ReviewVotes.Remove(vote!);
            review.AddHelpful(-1);
        }
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Hai lần bấm cùng lúc từ một trình duyệt: lần sau trùng khóa, giữ kết quả của lần trước
            throw new DomainException("Đã ghi nhận, vui lòng tải lại trang.");
        }
        return new HelpfulResult(review.HelpfulCount, on);
    }

    [GeneratedRegex(@"[\s.\-]")]
    private static partial Regex PhoneSeparators();

    [GeneratedRegex(@"^0\d{9}$")]
    private static partial Regex Phone();
}
