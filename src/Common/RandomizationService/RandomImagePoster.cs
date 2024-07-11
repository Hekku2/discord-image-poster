using DiscordImagePoster.Common.BlobStorageImageService;
using DiscordImagePoster.Common.Discord;
using DiscordImagePoster.Common.ImageAnalysis;
using DiscordImagePoster.Common.IndexService;
using Microsoft.Extensions.Logging;

namespace DiscordImagePoster.Common.RandomizationService;

public class RandomImagePoster : IRandomImagePoster
{
    private readonly ILogger<RandomImagePoster> _logger;
    private readonly IDiscordImagePoster _discordImagePoster;
    private readonly IBlobStorageImageService _imageService;
    private readonly IIndexService _indexService;
    private readonly IRandomizationService _randomizationService;
    private readonly IImageAnalysisService _imageAnalysisService;

    public RandomImagePoster(
        ILogger<RandomImagePoster> logger,
        IDiscordImagePoster discordImagePoster,
        IBlobStorageImageService imageService,
        IIndexService indexService,
        IRandomizationService randomizationService,
        IImageAnalysisService imageAnalysisService
    )
    {
        _logger = logger;
        _discordImagePoster = discordImagePoster;
        _imageService = imageService;
        _indexService = indexService;
        _randomizationService = randomizationService;
        _imageAnalysisService = imageAnalysisService;
    }

    public async Task PostRandomImageAsync()
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
