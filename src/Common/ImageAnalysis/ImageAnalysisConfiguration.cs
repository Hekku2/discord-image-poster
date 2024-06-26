namespace DiscordImagePoster.Common.ImageAnalysis;

/// <summary>
/// Configuration for image analysis service.
/// </summary>
public class ImageAnalysisConfiguration
{
    /// <summary>
    /// The endpoint of the image analysis service.
    /// For example https://service-name-here.cognitiveservices.azure.com/
    /// </summary>
    public required string Endpoint { get; set; }
}
