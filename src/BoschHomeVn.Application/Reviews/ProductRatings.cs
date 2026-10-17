using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Reviews;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Reviews;

internal static class ProductRatings
{
    // Tính lại điểm trung bình + số đánh giá của sản phẩm từ các đánh giá đang hiện (gọi sau khi đã lưu thay đổi đánh giá)
    public static async Task RefreshRatingAsync(this IAppDbContext db, string productId, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null)
        {
            return;
        }

        var approved = db.ProductReviews.Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved);
        var count = await approved.CountAsync(cancellationToken);
        var sum = count == 0 ? 0 : await approved.SumAsync(r => r.Rating, cancellationToken);
        product.SetRating(count == 0 ? null : Math.Round((decimal)sum / count, 1), count);
        await db.SaveChangesAsync(cancellationToken);
    }
}
