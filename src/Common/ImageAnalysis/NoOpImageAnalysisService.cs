using Microsoft.Extensions.Logging;

namespace DiscordImagePoster.Common.ImageAnalysis;

public class NoOpImageAnalysisService : IImageAnalysisService
{
    public NoOpImageAnalysisService(ILogger<NoOpImageAnalysisService> logger)
    {
    }

    public Task<ImageAnalysisResults?> AnalyzeImageAsync(BinaryData binaryData)
    {
        return Task.FromResult<ImageAnalysisResults?>(null);
    }
}
