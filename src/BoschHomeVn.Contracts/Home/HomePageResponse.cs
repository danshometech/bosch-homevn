using BoschHomeVn.Contracts.Catalog;

namespace BoschHomeVn.Contracts.Home;

public sealed record HomePageResponse(HomeHeroResponse? Hero, IReadOnlyList<HomeMomentResponse> Moments, FlashSaleResponse? FlashSale);

// Banner đầu trang: Title mỗi dòng một dòng chữ lớn; nút hiện khi có cả CtaLabel và CtaUrl
public sealed record HomeHeroResponse(
    string Time,
    string? Kicker,
    string Title,
    string? Lead,
    string? CtaLabel,
    string? CtaUrl,
    string ImageUrl,
    string? ImagePosition,
    string? ImageAlt);

public sealed record HomeMomentResponse(
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
    MomentFactResponse? Fact,
    IReadOnlyList<LinkResponse> Links,
    IReadOnlyList<ProductResponse> Products,
    MomentExtrasResponse Extras);

public sealed record MomentFactResponse(string Value, string? Note);

public sealed record MomentExtrasResponse(string? Title, IReadOnlyList<ProductResponse> Products, LinkResponse? More);

public sealed record LinkResponse(string Label, string Url);

public sealed record FlashSaleResponse(int Count, int MaxDiscountPercent, IReadOnlyList<string> Types);
