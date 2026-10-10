using BoschHomeVn.Application.Abstractions.Content;
using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Common;
using BoschHomeVn.Domain.News;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.News.Admin;

public sealed record SaveNewsPostCommand(
    string Title,
    string? Slug,
    string CategoryId,
    string? Summary,
    string? CoverImageUrl,
    string? Html,
    bool IsPublished);

public sealed class NewsAdminHandler(IAppDbContext db, IArticleSanitizer sanitizer)
{
    private const int MaxSlugLength = 200;

    public async Task<IReadOnlyList<NewsCategoryItem>> ListCategoriesAsync(CancellationToken cancellationToken)
    {
        var counts = await db.NewsPosts
            .GroupBy(p => p.CategoryId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        var categories = await db.NewsCategories.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync(cancellationToken);
        return [.. categories.Select(c => new NewsCategoryItem(c, counts.GetValueOrDefault(c.Id)))];
    }

    public async Task<string> CreateCategoryAsync(string name, string? description, CancellationToken cancellationToken)
    {
        var id = Slug.From(name ?? "");
        if (id.Length == 0)
        {
            throw new DomainException("Tên phải có ít nhất một chữ hoặc số.");
        }
        if (await db.NewsCategories.AnyAsync(c => c.Id == id, cancellationToken))
        {
            throw new DomainException($"Đã có chuyên mục mã \"{id}\".");
        }

        var last = await db.NewsCategories.MaxAsync(c => (int?)c.SortOrder, cancellationToken) ?? -1;
        db.NewsCategories.Add(new NewsCategory(id, name!, description, last + 1));
        await db.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task<bool> UpdateCategoryAsync(string id, string name, string? description, CancellationToken cancellationToken)
    {
        var category = await db.NewsCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        category.Update(name, description);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(string id, CancellationToken cancellationToken)
    {
        var category = await db.NewsCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return false;
        }
        if (await db.NewsPosts.AnyAsync(p => p.CategoryId == id, cancellationToken))
        {
            throw new DomainException("Chuyên mục còn bài viết — chuyển hoặc xóa hết bài trước.");
        }

        db.NewsCategories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ReorderCategoriesAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        var position = ids.Distinct().Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);
        var categories = await db.NewsCategories.ToListAsync(cancellationToken);
        var ordered = categories.OrderBy(c => position.GetValueOrDefault(c.Id, int.MaxValue)).ThenBy(c => c.SortOrder).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].SetSortOrder(i);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NewsPostSummary>> ListPostsAsync(CancellationToken cancellationToken) =>
        await (
            from p in db.NewsPosts.AsNoTracking()
            join c in db.NewsCategories on p.CategoryId equals c.Id
            orderby p.UpdatedAt descending
            select new NewsPostSummary(
                p.Id, p.Slug, p.Title, p.Summary, p.CoverImageUrl, p.CategoryId, c.Name, p.IsPublished, p.PublishedAt, p.UpdatedAt))
            .ToListAsync(cancellationToken);

    public Task<NewsPost?> GetPostAsync(Guid id, CancellationToken cancellationToken) =>
        db.NewsPosts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Guid> CreatePostAsync(SaveNewsPostCommand command, CancellationToken cancellationToken)
    {
        await EnsureCategoryAsync(command.CategoryId, cancellationToken);
        var slug = await UniqueSlugAsync(Guid.Empty, command, cancellationToken);
        var post = new NewsPost(
            command.CategoryId, command.Title, slug, command.Summary, command.CoverImageUrl, Clean(command.Html), DateTime.UtcNow);
        if (command.IsPublished)
        {
            post.Publish(DateTime.UtcNow);
        }

        db.NewsPosts.Add(post);
        await db.SaveChangesAsync(cancellationToken);
        return post.Id;
    }

    public async Task<bool> UpdatePostAsync(Guid id, SaveNewsPostCommand command, CancellationToken cancellationToken)
    {
        var post = await db.NewsPosts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (post is null)
        {
            return false;
        }

        await EnsureCategoryAsync(command.CategoryId, cancellationToken);
        var slug = await UniqueSlugAsync(id, command, cancellationToken);
        var now = DateTime.UtcNow;
        if (!command.IsPublished)
        {
            post.Unpublish();
        }
        post.Edit(command.CategoryId, command.Title, slug, command.Summary, command.CoverImageUrl, Clean(command.Html), now);
        if (command.IsPublished)
        {
            post.Publish(now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeletePostAsync(Guid id, CancellationToken cancellationToken)
    {
        var post = await db.NewsPosts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (post is null)
        {
            return false;
        }

        db.NewsPosts.Remove(post);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private string Clean(string? html) => sanitizer.Sanitize(html ?? "");

    private async Task EnsureCategoryAsync(string categoryId, CancellationToken cancellationToken)
    {
        if (!await db.NewsCategories.AnyAsync(c => c.Id == categoryId, cancellationToken))
        {
            throw new DomainException("Chuyên mục không tồn tại.");
        }
    }

    private async Task<string> UniqueSlugAsync(Guid id, SaveNewsPostCommand command, CancellationToken cancellationToken)
    {
        var auto = string.IsNullOrWhiteSpace(command.Slug);
        var slug = Slug.From(auto ? command.Title ?? "" : command.Slug!);
        if (slug.Length > MaxSlugLength)
        {
            slug = slug[..MaxSlugLength].TrimEnd('-');
        }
        if (slug.Length == 0)
        {
            throw new DomainException("Tiêu đề / đường dẫn phải có ít nhất một chữ hoặc số.");
        }

        var taken = await db.NewsPosts
            .Where(p => p.Id != id && p.Slug.StartsWith(slug))
            .Select(p => p.Slug)
            .ToListAsync(cancellationToken);
        if (!taken.Contains(slug))
        {
            return slug;
        }
        if (!auto)
        {
            throw new DomainException($"Đường dẫn \"{slug}\" đã có bài khác dùng.");
        }

        for (var i = 2; ; i++)
        {
            var next = $"{slug}-{i}";
            if (!taken.Contains(next))
            {
                return next;
            }
        }
    }
}
