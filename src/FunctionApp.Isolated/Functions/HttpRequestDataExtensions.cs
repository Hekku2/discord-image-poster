using Microsoft.Azure.Functions.Worker.Http;

namespace DiscordImagePoster.FunctionApp.Isolated;

public static class HttpRequestDataExtensions
{
    public static string? GetFirstHeaderValue(this HttpRequestData req, string headerName)
    {
        req.Headers.TryGetValues(headerName, out var headerValues);
        return headerValues?.FirstOrDefault();
    }
}

