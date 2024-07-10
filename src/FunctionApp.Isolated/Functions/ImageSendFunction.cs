using System.Net;
using DiscordImagePoster.Common.BlobStorageImageService;
using DiscordImagePoster.Common.Discord;
using DiscordImagePoster.Common.ImageAnalysis;
using DiscordImagePoster.Common.IndexService;
using DiscordImagePoster.Common.RandomizationService;
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
    private readonly IRandomizationService _randomizationService;
    private readonly IImageAnalysisService _imageAnalysisService;

    public ImageSendFunction(
        ILogger<ImageSendFunction> logger,
        IOptions<FeatureSettings> featureSettings,
        IDiscordImagePoster discordImagePoster,
        IBlobStorageImageService imageService,
        IIndexService indexService,
        IRandomizationService randomizationService,
        IImageAnalysisService imageAnalysisService
        )
    {
        _logger = logger;
        _featureSettings = featureSettings.Value;
        _discordImagePoster = discordImagePoster;
        _imageService = imageService;
        _indexService = indexService;
        _randomizationService = randomizationService;
        _imageAnalysisService = imageAnalysisService;
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
        var randomImage = _randomizationService.GetRandomImage(index);

        if (randomImage is null)
        {
            _logger.LogError("No images found in index.");
            return;
        }

        var result = await _imageService.GetImageStream(randomImage.Name);
        if (result is null)
        {
            _logger.LogError("No image found.");
            return;
        }

        var binaryData = BinaryData.FromStream(result.Content);
        var analyzationResults = await _imageAnalysisService.AnalyzeImageAsync(binaryData);

        _logger.LogInformation("Sending image {ImageName} with caption {Caption} and tags {Tags}", randomImage.Name, analyzationResults?.Caption, string.Join(", ", analyzationResults?.Tags ?? Array.Empty<string>()));
        var imageMetadata = new ImageMetadataUpdate
        {
            Name = randomImage.Name,
            Caption = analyzationResults?.Caption,
            Tags = analyzationResults?.Tags
        };

        var parameters = new ImagePostingParameters
        {
            ImageStream = binaryData.ToStream(),
            FileName = randomImage.Name,
            Description = analyzationResults?.Caption
        };
        await _discordImagePoster.SendImageAsync(parameters);

        await _indexService.IncreasePostingCountAndUpdateMetadataAsync(imageMetadata);
    }
}
