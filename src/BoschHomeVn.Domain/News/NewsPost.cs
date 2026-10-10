using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Domain.News;

public sealed class NewsPost : Entity<Guid>
{
    public const int MaxHtmlLength = 200_000;

    private NewsPost() { } // EF Core

    public NewsPost(string categoryId, string title, string slug, string? summary, string? coverImageUrl, string html, DateTime now)
    {
        Id = Guid.CreateVersion7();
        Edit(categoryId, title, slug, summary, coverImageUrl, html, now);
    }

    public string CategoryId { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Summary { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public string Html { get; private set; } = "";
    public bool IsPublished { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Edit(string categoryId, string title, string slug, string? summary, string? coverImageUrl, string html, DateTime now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        if (html.Length > MaxHtmlLength)
        {
            throw new DomainException($"Nội dung bài tối đa {MaxHtmlLength:N0} ký tự.");
        }

        CategoryId = categoryId;
        Title = title.Trim();
        Slug = slug;
        Summary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
        CoverImageUrl = string.IsNullOrWhiteSpace(coverImageUrl) ? null : coverImageUrl.Trim();
        Html = html;
        UpdatedAt = now;
        if (IsPublished && Html.Length == 0)
        {
            throw new DomainException("Bài đang đăng phải có nội dung.");
        }
    }

    public void Publish(DateTime now)
    {
        if (Html.Length == 0)
        {
            throw new DomainException("Bài chưa có nội dung, chưa đăng được.");
        }

        IsPublished = true;
        PublishedAt ??= now;
    }

    public void Unpublish() => IsPublished = false;
}
