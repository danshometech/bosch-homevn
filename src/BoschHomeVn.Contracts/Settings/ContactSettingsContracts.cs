using System.ComponentModel.DataAnnotations;

namespace BoschHomeVn.Contracts.Settings;

public sealed record ContactSettingsResponse(string Hotline, string? ZaloUrl);

public sealed record SaveContactSettingsRequest([StringLength(20)] string Hotline, [StringLength(300)] string? ZaloUrl);
