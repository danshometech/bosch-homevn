using System.Text.RegularExpressions;
using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Common;
using BoschHomeVn.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Settings;

public sealed record ContactSettings(string Hotline, string? ZaloUrl);

public sealed partial class SiteSettingsHandler(IAppDbContext db)
{
    public const string DefaultHotline = "1900 6868";

    public async Task<ContactSettings> GetContactAsync(CancellationToken cancellationToken)
    {
        var values = await db.SiteSettings
            .AsNoTracking()
            .Where(s => s.Key == SiteSetting.Hotline || s.Key == SiteSetting.ZaloUrl)
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);
        return new ContactSettings(values.GetValueOrDefault(SiteSetting.Hotline) ?? DefaultHotline, values.GetValueOrDefault(SiteSetting.ZaloUrl));
    }

    public async Task<ContactSettings> SaveContactAsync(ContactSettings settings, CancellationToken cancellationToken)
    {
        await SetAsync(SiteSetting.Hotline, NormalizeHotline(settings.Hotline), cancellationToken);
        await SetAsync(SiteSetting.ZaloUrl, NormalizeZalo(settings.ZaloUrl), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await GetContactAsync(cancellationToken);
    }

    private async Task SetAsync(string key, string? value, CancellationToken cancellationToken)
    {
        var row = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
        if (value is null)
        {
            if (row is not null)
            {
                db.SiteSettings.Remove(row);
            }
        }
        else if (row is null)
        {
            db.SiteSettings.Add(new SiteSetting(key, value));
        }
        else
        {
            row.Set(value);
        }
    }

    private static string NormalizeHotline(string? input)
    {
        var value = Spaces().Replace(input?.Trim() ?? "", " ");
        if (!PhoneLike().IsMatch(value) || NonDigit().Replace(value, "").Length < 4)
        {
            throw new DomainException("Hotline chỉ gồm chữ số, khoảng trắng, dấu chấm, gạch nối, vd. 1900 6868 hoặc 0912 345 678.");
        }
        return value;
    }

    private static string? NormalizeZalo(string? input)
    {
        var value = input?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        if (PhoneLike().IsMatch(value))
        {
            return "https://zalo.me/" + NonDigit().Replace(value, "");
        }

        if (!value.Contains("://"))
        {
            value = "https://" + value;
        }
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && uri.Scheme is "https" or "http"
            && uri.Host is "zalo.me" or "www.zalo.me" or "oa.zalo.me"
            && uri.AbsolutePath.Length > 1)
        {
            return "https://" + uri.Host.Replace("www.", "") + uri.PathAndQuery;
        }

        throw new DomainException("Nhập số điện thoại Zalo hoặc link zalo.me, vd. 0912345678 hoặc https://zalo.me/0912345678.");
    }

    [GeneratedRegex(@"^\+?[\d\s.\-()]{4,20}$")]
    private static partial Regex PhoneLike();

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigit();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();
}
