using BoschHomeVn.Application.News;
using BoschHomeVn.Application.News.Admin;
using BoschHomeVn.Contracts.Admin;
using BoschHomeVn.Contracts.News;
using BoschHomeVn.Domain.News;

namespace BoschHomeVn.Api.Mappings;

internal static class NewsMappings
{
    public static NewsCategoryResponse ToResponse(this NewsCategoryItem x) =>
        new(x.Category.Id, x.Category.Name, x.Category.Description, x.PostCount);

    public static NewsPostSummaryResponse ToResponse(this NewsPostSummary p) =>
        new(p.Slug, p.Title, p.Summary, p.CoverImageUrl, p.CategoryId, p.CategoryName, p.PublishedAt ?? p.UpdatedAt);

    public static NewsPostResponse ToResponse(this NewsPostDetail d)
    {
        var p = d.Post;
        return new NewsPostResponse(
            p.Slug, p.Title, p.Summary, p.CoverImageUrl, p.CategoryId, p.CategoryName, p.PublishedAt ?? p.UpdatedAt,
            d.Html, [.. d.Related.Select(r => r.ToResponse())]);
    }

    public static AdminNewsCategoryResponse ToAdminResponse(this NewsCategoryItem x) =>
        new(x.Category.Id, x.Category.Name, x.Category.Description, x.PostCount);

    public static AdminNewsPostListItem ToAdminResponse(this NewsPostSummary p) =>
        new(p.Id, p.Slug, p.Title, p.CoverImageUrl, p.CategoryId, p.CategoryName, p.IsPublished, p.PublishedAt, p.UpdatedAt);

    public static AdminNewsPostResponse ToAdminResponse(this NewsPost p) =>
        new(p.Id, p.Slug, p.Title, p.Summary, p.CoverImageUrl, p.CategoryId, p.Html, p.IsPublished, p.PublishedAt, p.UpdatedAt);

    public static SaveNewsPostCommand ToCommand(this SaveNewsPostRequest r) =>
        new(r.Title, r.Slug, r.CategoryId, r.Summary, r.CoverImageUrl, r.Html, r.IsPublished);
}
