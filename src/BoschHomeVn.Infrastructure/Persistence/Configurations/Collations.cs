namespace BoschHomeVn.Infrastructure.Persistence.Configurations;

internal static class Collations
{
    // Không phân biệt hoa/thường và dấu, "đ" coi như "d" (Vietnamese_* thì vẫn phân biệt dấu thanh)
    public const string AccentInsensitive = "Latin1_General_100_CI_AI";
}
