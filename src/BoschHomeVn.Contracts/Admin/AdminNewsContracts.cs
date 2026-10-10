using System.ComponentModel.DataAnnotations;

namespace BoschHomeVn.Contracts.Admin;

public sealed record AdminNewsCategoryResponse(string Id, string Name, string? Description, int PostCount);

public sealed record SaveNewsCategoryRequest([StringLength(200)] string Name, [StringLength(300)] string? Description);

public sealed record AdminNewsPostListItem(
    Guid Id,
    string Slug,
    string Title,
    string? CoverImageUrl,
    string CategoryId,
    string CategoryName,
    bool IsPublished,
    DateTime? PublishedAt,
    DateTime UpdatedAt);

public sealed record AdminNewsPostResponse(
    Guid Id,
    string Slug,
    string Title,
    string? Summary,
    string? CoverImageUrl,
    string CategoryId,
    string Html,
    bool IsPublished,
    DateTime? PublishedAt,
    DateTime UpdatedAt);

public sealed record SaveNewsPostRequest(
    [StringLength(300)] string Title,
    [StringLength(200)] string? Slug,
    [StringLength(64)] string CategoryId,
    [StringLength(500)] string? Summary,
    [StringLength(500)] string? CoverImageUrl,
    [StringLength(200_000)] string? Html,
    bool IsPublished);
