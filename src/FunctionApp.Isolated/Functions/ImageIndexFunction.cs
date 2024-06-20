using DiscordImagePoster.Common.IndexService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DiscordImagePoster.FunctionApp.Isolated;

public class ImageIndexFunction
{
    private readonly ILogger _logger;
    private readonly IIndexService _indexService;

    public ImageIndexFunction(ILogger<ImageSendFunction> logger, IIndexService indexService)
    {
        _logger = logger;
        _indexService = indexService;
    }

    [Function(nameof(GetImageIndex))]
    public async Task<ImageIndex?> GetImageIndex([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Getting index.");
        return await _indexService.GetIndexAsync();
    }

    [Function(nameof(UpdateImageIndex))]
    public async Task UpdateImageIndex([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("Updating image index");
        await _indexService.RefreshIndexAsync();
    }
}
