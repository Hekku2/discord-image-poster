namespace DiscordImagePoster.Common.ImageAnalysis;

/// <summary>
/// Service for analyzing images.
/// </summary>
public interface IImageAnalysisService
{
    /// <summary>
    /// Analyzes the image in stream.
    /// </summary>
    /// <param name="stream">The stream containing the image.</param>
    /// <returns>The analysis results.</returns>
    Task<ImageAnalysisResults> AnalyzeImageAsync(Stream stream);
}
