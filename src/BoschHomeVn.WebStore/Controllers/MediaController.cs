using System.Net;
using System.Text.RegularExpressions;
using BoschHomeVn.WebStore.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Controllers;

public sealed partial class MediaController(StoreApiClient api) : ControllerBase
{
    private static readonly string[] ForwardHeaders = ["Range", "If-Range", "If-None-Match", "If-Modified-Since"];

    [HttpGet("media/{**path}")]
    public async Task Get(string path, CancellationToken cancellationToken)
    {
        if (!SafePath().IsMatch(path) || path.Contains(".."))
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        try
        {
            using var response = await api.SendMediaAsync(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "media/" + path);
                foreach (var name in ForwardHeaders)
                {
                    if (Request.Headers.TryGetValue(name, out var value))
                    {
                        request.Headers.TryAddWithoutValidation(name, (IEnumerable<string?>)value);
                    }
                }
                return request;
            }, cancellationToken);
            using var sent = response?.RequestMessage;
            if (response is null)
            {
                Response.StatusCode = StatusCodes.Status502BadGateway;
                return;
            }

            Response.StatusCode = (int)response.StatusCode;
            if (response.Content.Headers.ContentRange is { } range)
            {
                Response.Headers.ContentRange = range.ToString();
            }
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotModified)
            {
                return;
            }

            Response.Headers.CacheControl = "public, max-age=86400";
            Response.Headers.AcceptRanges = "bytes";
            if (response.Headers.ETag is { } etag)
            {
                Response.Headers.ETag = etag.ToString();
            }
            if (response.Content.Headers.LastModified is { } lastModified)
            {
                Response.Headers.LastModified = lastModified.ToString("R");
            }
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return;
            }

            Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            Response.ContentLength = response.Content.Headers.ContentLength;
            await response.Content.CopyToAsync(Response.Body, cancellationToken);
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    [GeneratedRegex("^[A-Za-z0-9._/-]+$")]
    private static partial Regex SafePath();
}
