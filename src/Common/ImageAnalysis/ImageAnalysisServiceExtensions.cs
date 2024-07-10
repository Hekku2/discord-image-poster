using Azure.AI.Vision.ImageAnalysis;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiscordImagePoster.Common.ImageAnalysis;

public static class ImageAnalysisServiceExtensions
{
    /// <summary>
    /// Register the Image analysis service.
    /// </summary>
    /// <param name="hostbuilder">HostBuilder</param>
    /// <returns>Hostbuilder</returns>
    public static IServiceCollection AddImageAnalysisServices(this IServiceCollection services)
    {
        services.AddOptions<ImageAnalysisConfiguration>().BindConfiguration(nameof(ImageAnalysisConfiguration)).ValidateDataAnnotations().ValidateOnStart();
        services.AddTransient(services =>
        {
            var options = services.GetRequiredService<IOptions<ImageAnalysisConfiguration>>().Value;
            return new ImageAnalysisClient(new Uri(options.Endpoint), new DefaultAzureCredential());
        });
        services.AddTransient<IImageAnalysisService, ImageAnalysisService>();
        return services;
    }
}
