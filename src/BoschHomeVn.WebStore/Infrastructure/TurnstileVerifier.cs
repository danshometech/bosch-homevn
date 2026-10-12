using System.Text.Json.Serialization;

namespace BoschHomeVn.WebStore.Infrastructure;

// Cloudflare Turnstile chống spam đặt hàng: trình duyệt lấy token qua widget (SiteKey công khai), host gửi token kèm
// SecretKey lên Cloudflare kiểm tra (siteverify). Token chỉ dùng được một lần, hết hạn sau 5 phút.
// Cấu hình Turnstile:SiteKey / Turnstile:SecretKey; thiếu một trong hai thì tắt (Enabled = false, không kiểm tra).
public sealed class TurnstileVerifier(HttpClient http, IConfiguration config, ILogger<TurnstileVerifier> logger)
{
    private const string VerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    public bool Enabled => SiteKey is not null && SecretKey is not null;
    public string? SiteKey => Value("Turnstile:SiteKey");
    private string? SecretKey => Value("Turnstile:SecretKey");

    public async Task<bool> VerifyAsync(string? token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 2048)
        {
            return false;
        }

        try
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string> { ["secret"] = SecretKey!, ["response"] = token });
            using var response = await http.PostAsync(VerifyUrl, content, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<SiteverifyResult>(cancellationToken);
            if (result?.Success == true)
            {
                logger.LogInformation("Turnstile xác minh thành công: {Hostname}, giải lúc {ChallengeTs}", result.Hostname, result.ChallengeTs);
                return true;
            }

            logger.LogInformation("Turnstile từ chối token: {Errors}", string.Join(", ", result?.ErrorCodes ?? []));
            return false;
        }
        catch (Exception e) when ((e is HttpRequestException or TaskCanceledException) && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(e, "Không gọi được Cloudflare Turnstile");
            return false;
        }
    }

    private string? Value(string key) => string.IsNullOrWhiteSpace(config[key]) ? null : config[key];

    private sealed record SiteverifyResult(
        bool Success,
        [property: JsonPropertyName("error-codes")] string[]? ErrorCodes,
        string? Hostname,
        [property: JsonPropertyName("challenge_ts")] string? ChallengeTs);
}
