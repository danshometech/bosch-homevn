using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.News;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.News;

public sealed record NewsCategoryItem(NewsCategory Category, int PostCount);

public sealed record NewsPostSummary(
    Guid Id,
    string Slug,
    string Title,
    string? Summary,
    string? CoverImageUrl,
    string CategoryId,
    string CategoryName,
    bool IsPublished,
    DateTime? PublishedAt,
    DateTime UpdatedAt);

public sealed record NewsPostPage(IReadOnlyList<NewsPostSummary> Items, int Total);

public sealed record NewsPostDetail(NewsPostSummary Post, string Html, IReadOnlyList<NewsPostSummary> Related);

public sealed class NewsHandler(IAppDbContext db)
{
    public async Task<IReadOnlyList<NewsCategoryItem>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        var counts = await db.NewsPosts
            .Where(p => p.IsPublished)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        var categories = await db.NewsCategories.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync(cancellationToken);
        return [.. categories.Select(c => new NewsCategoryItem(c, counts.GetValueOrDefault(c.Id)))];
    }

    public async Task<NewsPostPage> GetPostsAsync(string? categoryId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = Published(categoryId, null);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new NewsPostPage(items, total);
    }

    public async Task<NewsPostDetail?> GetPostAsync(string slug, CancellationToken cancellationToken)
    {
        var post = await db.NewsPosts.AsNoTracking().FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished, cancellationToken);
        if (post is null)
        {
            return null;
        }

        var categoryName = await db.NewsCategories
            .Where(c => c.Id == post.CategoryId)
            .Select(c => c.Name)
            .FirstAsync(cancellationToken);
        var related = await Published(post.CategoryId, post.Id).Take(3).ToListAsync(cancellationToken);
        var summary = new NewsPostSummary(
            post.Id, post.Slug, post.Title, post.Summary, post.CoverImageUrl, post.CategoryId, categoryName,
            post.IsPublished, post.PublishedAt, post.UpdatedAt);
        return new NewsPostDetail(summary, post.Html, related);
    }

    private IQueryable<NewsPostSummary> Published(string? categoryId, Guid? exceptId) =>
        from p in db.NewsPosts.AsNoTracking()
        join c in db.NewsCategories on p.CategoryId equals c.Id
        where p.IsPublished && (categoryId == null || p.CategoryId == categoryId) && (exceptId == null || p.Id != exceptId)
        orderby p.PublishedAt descending, p.Id descending
        select new NewsPostSummary(
            p.Id, p.Slug, p.Title, p.Summary, p.CoverImageUrl, p.CategoryId, c.Name, p.IsPublished, p.PublishedAt, p.UpdatedAt);
}
