using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using BoschHomeVn.Application.Abstractions.Address;
using Microsoft.Extensions.Caching.Memory;

namespace BoschHomeVn.Infrastructure.Address;

// Lấy từ API miễn phí provinces.open-api.vn (v2), giữ trong bộ nhớ 24 giờ — danh mục gần như không đổi.
// Sắp theo tên bỏ tiền tố ("Thành phố", "Tỉnh", "Phường", "Xã", "Đặc khu") để dễ tìm
internal sealed partial class OpenApiAddressDirectory(HttpClient http, IMemoryCache cache) : IAddressDirectory
{
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);
    private static readonly CompareInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN").CompareInfo;

    public async Task<IReadOnlyList<AdministrativeUnit>> GetProvincesAsync(CancellationToken cancellationToken) =>
        (await cache.GetOrCreateAsync("address:provinces", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = Ttl;
            var list = await http.GetFromJsonAsync<List<UnitDto>>("p/", cancellationToken) ?? [];
            return Sort(list);
        }))!;

    public async Task<IReadOnlyList<AdministrativeUnit>?> GetWardsAsync(int provinceCode, CancellationToken cancellationToken)
    {
        var key = $"address:wards:{provinceCode}";
        if (cache.TryGetValue(key, out IReadOnlyList<AdministrativeUnit>? hit))
        {
            return hit;
        }

        using var response = await http.GetAsync($"p/{provinceCode}?depth=2", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();

        var province = await response.Content.ReadFromJsonAsync<ProvinceDto>(cancellationToken);
        var wards = Sort(province?.Wards ?? []);
        cache.Set(key, wards, Ttl);
        return wards;
    }

    private static IReadOnlyList<AdministrativeUnit> Sort(IEnumerable<UnitDto> units) =>
        [.. units
            .Select(u => new AdministrativeUnit(u.Code, u.Name.Trim()))
            .OrderBy(u => Prefix().Replace(u.Name, ""), Comparer<string>.Create((a, b) => Vietnamese.Compare(a, b, CompareOptions.IgnoreCase)))];

    [GeneratedRegex(@"^(Thành phố|Tỉnh|Phường|Xã|Đặc khu)\s+", RegexOptions.IgnoreCase)]
    private static partial Regex Prefix();

    private sealed record UnitDto(int Code, string Name);

    private sealed record ProvinceDto(int Code, string Name, List<UnitDto>? Wards);
}
