using BoschHomeVn.Application.Abstractions.Content;
using Ganss.Xss;

namespace BoschHomeVn.Infrastructure.Content;

internal sealed class ArticleSanitizer : IArticleSanitizer
{
    private readonly HtmlSanitizer sanitizer = new();

    public ArticleSanitizer()
    {
        sanitizer.AllowedSchemes.Add("mailto");
        sanitizer.AllowedSchemes.Add("tel");
    }

    public string Sanitize(string html) => sanitizer.Sanitize(html).Trim();
}
