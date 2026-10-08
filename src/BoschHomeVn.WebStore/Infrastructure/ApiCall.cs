using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Infrastructure;

// Gọi Api không ném exception (debug không bị dừng): Api trả lỗi → giữ nguyên mã + ProblemDetails, không gọi được Api
// → 502. GET thử lại khi chưa kết nối được (Api đang khởi động, container vừa restart).
internal static class ApiCall
{
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)];

    public static async Task<ActionResult<T>> SendAsync<T>(
        HttpClient http, Func<HttpRequestMessage> create, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await SendWithRetryAsync(http, create, HttpCompletionOption.ResponseContentRead, cancellationToken);
            using var request = response.RequestMessage;
            if (!response.IsSuccessStatusCode)
            {
                return await ProblemAsync(response, cancellationToken);
            }
            return (await response.Content.ReadFromJsonAsync<T>(cancellationToken))!;
        }
        catch (Exception e) when (IsUnavailable(e, cancellationToken))
        {
            return Unavailable(e, logger);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
    }

    // Ảnh / video: trả response đọc dần để stream; null = không gọi được Api
    public static async Task<HttpResponseMessage?> SendMediaAsync(
        HttpClient http, Func<HttpRequestMessage> create, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            return await SendWithRetryAsync(http, create, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (Exception e) when (IsUnavailable(e, cancellationToken))
        {
            logger.LogWarning(e, "Không gọi được BoschHomeVn.Api");
            return null;
        }
    }

    private static async Task<HttpResponseMessage> SendWithRetryAsync(
        HttpClient http, Func<HttpRequestMessage> create, HttpCompletionOption option, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            var request = create();
            try
            {
                return await http.SendAsync(request, option, cancellationToken);
            }
            catch (HttpRequestException) when (request.Method == HttpMethod.Get && attempt < RetryDelays.Length)
            {
                request.Dispose();
                await Task.Delay(RetryDelays[attempt], cancellationToken);
            }
        }
    }

    // Không kết nối được, hoặc quá thời gian chờ (không phải do trình duyệt bỏ request)
    private static bool IsUnavailable(Exception e, CancellationToken cancellationToken) =>
        !cancellationToken.IsCancellationRequested && e is HttpRequestException or TaskCanceledException;

    private static async Task<ActionResult> ProblemAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        response.Content.Headers.ContentType?.MediaType?.Contains("json") == true
            ? new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = await response.Content.ReadAsStringAsync(cancellationToken),
                ContentType = "application/problem+json",
            }
            : new StatusCodeResult((int)response.StatusCode);

    private static ObjectResult Unavailable(Exception e, ILogger logger)
    {
        logger.LogWarning(e, "Không gọi được BoschHomeVn.Api");
        return new ObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status502BadGateway,
            Title = "Không kết nối được máy chủ dữ liệu, vui lòng thử lại sau.",
        })
        { StatusCode = StatusCodes.Status502BadGateway };
    }
}
