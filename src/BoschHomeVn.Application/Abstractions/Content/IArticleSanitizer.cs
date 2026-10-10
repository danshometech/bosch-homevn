namespace BoschHomeVn.Application.Abstractions.Content;

public interface IArticleSanitizer
{
    string Sanitize(string html);
}
