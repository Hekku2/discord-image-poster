using System.Text.Json;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Extensions.Logging;

namespace DiscordImagePoster.Common.ImageAnalysis;

/// <summary>
/// Image analysis service using Azure Cognitive Services.
/// </summary>
public class ImageAnalysisService : IImageAnalysisService
{
    private readonly ILogger<ImageAnalysisService> _logger;
    private readonly ImageAnalysisClient _imageAnalysisClient;

    public ImageAnalysisService(
        ILogger<ImageAnalysisService> logger,
        ImageAnalysisClient imageAnalysisClient)
    {
        _logger = logger;
        _imageAnalysisClient = imageAnalysisClient;
    }

    /// <inheritdoc/>
    public async Task<ImageAnalysisResults?> AnalyzeImageAsync(BinaryData binaryData)
    {
        _logger.LogDebug("Analyzing image...");
        var result = await _imageAnalysisClient.AnalyzeAsync(binaryData, VisualFeatures.Caption | VisualFeatures.Tags);
        var tags = result.Value.Tags.Values.Select(tag => $"{tag.Name} {tag.Confidence}").ToArray();
        _logger.LogDebug("Image analysis result with caption: {result}, and tags {tags}", result.Value.Caption.Text, tags);
        _logger.LogTrace("Image analysis result: {result}", JsonSerializer.Serialize(result));

        return new ImageAnalysisResults
        {
            Caption = result.Value.Caption.Text,
            Tags = result.Value.Tags.Values.Select(tag => tag.Name).ToArray()
        };
    }
}
