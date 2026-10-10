using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Domain.Catalog;

public sealed class ProductArticle
{
    public const int MaxLength = 200_000;

    private ProductArticle() { } // EF Core

    public ProductArticle(string productId, string html)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        ProductId = productId;
        Update(html);
    }

    public string ProductId { get; private set; } = null!;
    public string Html { get; private set; } = null!;

    public void Update(string html)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(html);
        if (html.Length > MaxLength)
        {
            throw new DomainException($"Bài giới thiệu tối đa {MaxLength:N0} ký tự.");
        }

        Html = html;
    }
}
