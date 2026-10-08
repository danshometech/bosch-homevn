using System.ComponentModel.DataAnnotations;
using BoschHomeVn.Contracts.Home;

namespace BoschHomeVn.Contracts.Admin;

// Khoảnh khắc trang chủ cho trang quản trị: ProductIds = hàng thẻ chính, ExtraProductIds = dải "Mua kèm" (theo thứ tự)
public sealed record AdminMomentResponse(
    string Id,
    string Time,
    string Name,
    string Tone,
    string Title,
    string Lead,
    string? ImageUrl,
    string? ImagePosition,
    string? StillImageUrl,
    string? ImageAlt,
    string? FactProductId,
    string? FactSpecKey,
    string? FactNote,
    IReadOnlyList<LinkResponse> Links,
    string? ExtrasTitle,
    string? ExtrasMoreLabel,
    string? ExtrasMoreUrl,
    IReadOnlyList<string> ProductIds,
    IReadOnlyList<string> ExtraProductIds,
    IReadOnlyList<string> Tones);

public sealed record SaveMomentRequest(
    [StringLength(5)] string Time,
    [StringLength(100)] string Name,
    [StringLength(20)] string Tone,
    [StringLength(200)] string Title,
    [StringLength(1000)] string Lead,
    [StringLength(500)] string? ImageUrl,
    [StringLength(50)] string? ImagePosition,
    [StringLength(500)] string? StillImageUrl,
    [StringLength(300)] string? ImageAlt,
    [StringLength(64)] string? FactProductId,
    [StringLength(100)] string? FactSpecKey,
    [StringLength(300)] string? FactNote,
    IReadOnlyList<LinkRequest> Links,
    [StringLength(200)] string? ExtrasTitle,
    [StringLength(200)] string? ExtrasMoreLabel,
    [StringLength(500)] string? ExtrasMoreUrl,
    IReadOnlyList<string> ProductIds,
    IReadOnlyList<string> ExtraProductIds);

public sealed record LinkRequest([StringLength(200)] string Label, [StringLength(500)] string Url);
