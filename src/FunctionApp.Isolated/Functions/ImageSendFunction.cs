using System.Net;
using DiscordImagePoster.Common.BlobStorageImageService;
using DiscordImagePoster.Common.Discord;
using DiscordImagePoster.Common.IndexService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordImagePoster.FunctionApp.Isolated;

public class ImageSendFunction
{
    private readonly ILogger _logger;
    private readonly FeatureSettings _featureSettings;
    private readonly IDiscordImagePoster _discordImagePoster;
    private readonly IBlobStorageImageService _imageService;
    private readonly IIndexService _indexService;

    public ImageSendFunction(
        ILogger<ImageSendFunction> logger,
        IOptions<FeatureSettings> featureSettings,
        IDiscordImagePoster discordImagePoster,
        IBlobStorageImageService imageService,
        IIndexService indexService)
    {
        _logger = logger;
        _featureSettings = featureSettings.Value;
        _discordImagePoster = discordImagePoster;
        _imageService = imageService;
        _indexService = indexService;
    }

    [Function("SendImage")]
    public async Task<HttpResponseData> SendRandomImage([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("Sending random image triggered manually.");

        await SendRandomImage();

        return req.CreateResponse(HttpStatusCode.Accepted);
    }

    [Function("SendRandomImage")]
    public async Task TriggerTimerSendRandomImage([TimerTrigger("0 0 */4 * * *")] TimerInfo timer)
    {
        _logger.LogDebug("Sending timed random image");
        if (_featureSettings.DisableTimedSending)
        {
            _logger.LogInformation("No timed sending enabled, skipping.");
            return;
        }

        await SendRandomImage();

        if (timer.ScheduleStatus is not null)
        {
            _logger.LogDebug("Next timer schedule at: {next}", timer.ScheduleStatus.Next);
        }
    }

    private async Task SendRandomImage()
    {
        var index = await _indexService.GetIndexOrCreateNew();
        var randomImage = index.Images.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

        if (randomImage is null)
        {
            _logger.LogError("No images found in index.");
            return;
        }

        var result = await _imageService.GetImageStream(randomImage.Name);
        if (result is null)
        {
            _logger.LogError("No image found");
            return;
        }
        await _discordImagePoster.SendImage(result.Content, randomImage.Name, randomImage.Description);
        await _indexService.IncreasePostingCountAsync(randomImage.Name);
    }
}
