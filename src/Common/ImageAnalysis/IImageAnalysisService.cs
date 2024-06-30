namespace DiscordImagePoster.Common.ImageAnalysis;

/// <summary>
/// Service for analyzing images.
/// </summary>
public interface IImageAnalysisService
{
    /// <summary>
    /// Analyzes the image provided in BinaryData.
    /// </summary>
    /// <param name="binaryData">The BinaryData containing the image.</param>
    /// <returns>The analysis results. Can be null if image analysis is not done.</returns>
    Task<ImageAnalysisResults?> AnalyzeImageAsync(BinaryData binaryData);
}
