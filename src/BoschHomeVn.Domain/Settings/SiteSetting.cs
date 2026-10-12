namespace BoschHomeVn.Domain.Settings;

public sealed class SiteSetting
{
    public const string ZaloUrl = "contact.zalo-url";
    public const string Hotline = "contact.hotline";
    public const string HomeHero = "home.hero";

    public const int MaxValueLength = 2000;

    private SiteSetting() { } // EF Core

    public SiteSetting(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        Key = key;
        Set(value);
    }

    public string Key { get; private set; } = null!;
    public string Value { get; private set; } = null!;

    public void Set(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }
}
