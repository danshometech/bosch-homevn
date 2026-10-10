namespace BoschHomeVn.Contracts.News;

public sealed record NewsCategoryResponse(string Id, string Name, string? Description, int PostCount);

public sealed record NewsPostSummaryResponse(
    string Slug,
    string Title,
    string? Summary,
    string? CoverImageUrl,
    string CategoryId,
    string CategoryName,
    DateTime PublishedAt);

public sealed record NewsPostPageResponse(IReadOnlyList<NewsPostSummaryResponse> Items, int Total, int Page, int PageSize);

public sealed record NewsPostResponse(
    string Slug,
    string Title,
    string? Summary,
    string? CoverImageUrl,
    string CategoryId,
    string CategoryName,
    DateTime PublishedAt,
    string Html,
    IReadOnlyList<NewsPostSummaryResponse> Related);
