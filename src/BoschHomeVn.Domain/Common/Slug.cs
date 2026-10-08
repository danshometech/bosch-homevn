using System.Globalization;
using System.Text;

namespace BoschHomeVn.Domain.Common;

// Slug dùng làm Id danh mục / loại trên URL — cùng quy tắc với slugify() của frontend (utils/format.js):
// bỏ dấu, "đ" → "d", chữ thường, ký tự khác a-z0-9 thành "-". "Máy rửa bát" → "may-rua-bat"
public static class Slug
{
    public static string From(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var c = ch is 'đ' or 'Đ' ? 'd' : char.ToLowerInvariant(ch);
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                sb.Append(c);
            }
            else if (sb.Length > 0 && sb[^1] != '-')
            {
                sb.Append('-');
            }
        }

        return sb.ToString().TrimEnd('-');
    }
}
