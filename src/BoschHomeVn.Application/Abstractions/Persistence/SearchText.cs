using System.Globalization;
using System.Text;

namespace BoschHomeVn.Application.Abstractions.Persistence;

// Tìm không dấu, không phân biệt hoa thường ("may rua bat" ra "Máy rửa bát", "dien" ra "điện").
// Fold chỉ dùng trong truy vấn EF — Infrastructure dịch sang SQL lower(unaccent(...)); Normalize làm điều tương tự
// trong .NET cho từ khóa người dùng gõ.
public static class SearchText
{
    public static string Fold(string value) => throw new NotSupportedException("SearchText.Fold chỉ dùng trong truy vấn EF.");

    public static string Normalize(string value)
    {
        var decomposed = value.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
