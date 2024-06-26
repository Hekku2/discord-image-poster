namespace DiscordImagePoster.Common.ImageAnalysis;

/// <summary>
/// Result of image analysis. Basically a subset of the data received from the service.
/// </summary>
public class ImageAnalysisResults
{
    /// <summary>
    /// The caption of the image.
    /// </summary>
    public required string Caption { get; set; }

    /// <summary>
    /// The tags of the image.
    /// </summary>
    public required string[] Tags { get; set; }
}
