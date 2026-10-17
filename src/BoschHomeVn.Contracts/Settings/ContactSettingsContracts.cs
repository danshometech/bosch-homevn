using System.ComponentModel.DataAnnotations;

namespace BoschHomeVn.Contracts.Settings;

public sealed record ContactSettingsResponse(string Hotline, string? ZaloUrl, string? FacebookUrl, string? TikTokUrl, string? YouTubeUrl);

public sealed record SaveContactSettingsRequest(
    [StringLength(20)] string Hotline,
    [StringLength(300)] string? ZaloUrl,
    [StringLength(300)] string? FacebookUrl,
    [StringLength(300)] string? TikTokUrl,
    [StringLength(300)] string? YouTubeUrl);
