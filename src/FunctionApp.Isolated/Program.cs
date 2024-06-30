using Azure.AI.Vision.ImageAnalysis;
using Azure.Identity;
using Azure.Storage.Blobs;
using DiscordImagePoster.Common;
using DiscordImagePoster.Common.BlobStorageImageService;
using DiscordImagePoster.Common.Discord;
using DiscordImagePoster.Common.ImageAnalysis;
using DiscordImagePoster.Common.IndexService;
using DiscordImagePoster.Common.RandomizationService;
using DiscordImagePoster.FunctionApp.Isolated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddOptions<BlobStorageImageSourceOptions>().BindConfiguration(nameof(BlobStorageImageSourceOptions)).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<DiscordConfiguration>().BindConfiguration(nameof(DiscordConfiguration)).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<FeatureSettings>().BindConfiguration(nameof(FeatureSettings)).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<ImageIndexOptions>().BindConfiguration(nameof(ImageIndexOptions)).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<ImageAnalysisConfiguration>().BindConfiguration(nameof(ImageAnalysisConfiguration)).ValidateDataAnnotations().ValidateOnStart();

        services.AddTransient(services =>
        {
            var options = services.GetRequiredService<IOptions<ImageAnalysisConfiguration>>().Value;
            return new ImageAnalysisClient(new Uri(options.Endpoint), new DefaultAzureCredential());
        });
        services.AddTransient<IImageAnalysisService>(services =>
        {
            var options = services.GetRequiredService<IOptions<FeatureSettings>>().Value;
            return
                options.DisableImageAnalysis ?
                new NoOpImageAnalysisService(services.GetRequiredService<ILogger<NoOpImageAnalysisService>>())
                :
                new ImageAnalysisService(services.GetRequiredService<ILogger<ImageAnalysisService>>(), services.GetRequiredService<ImageAnalysisClient>());
        });

        services.AddTransient<IDiscordImagePoster>(services =>
        {
            var options = services.GetRequiredService<IOptions<FeatureSettings>>().Value;
            return options.DisableDiscordSending ?
                new NoOpDiscordImagePoster(services.GetRequiredService<ILogger<NoOpDiscordImagePoster>>())
                :
                new DiscordImagePoster.Common.Discord.DiscordImagePoster(
                services.GetRequiredService<ILogger<DiscordImagePoster.Common.Discord.DiscordImagePoster>>(),
                services.GetRequiredService<IOptions<DiscordConfiguration>>());
        });

        services.AddTransient<IBlobStorageImageService, BlobStorageImageService>();
        services.AddTransient<IIndexStorageService, BlobStorageIndexStorageService>();
        services.AddTransient<IIndexService, IndexService>();
        services.AddTransient<IRandomizationService, RandomizationService>();

        services.AddKeyedTransient(KeyedServiceConstants.ImageBlobContainerClient, (services, _) =>
        {
            var options = services.GetRequiredService<IOptions<BlobStorageImageSourceOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BlobContainerUri))
            {
                return new BlobContainerClient(new Uri(options.BlobContainerUri), new DefaultAzureCredential());
            }

            return new BlobContainerClient(options.ConnectionString, options.ContainerName);
        });
        services.AddKeyedTransient(KeyedServiceConstants.ImageIndexBlobContainerClient, (services, _) =>
        {
            var options = services.GetRequiredService<IOptions<ImageIndexOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BlobContainerUri))
            {
                return new BlobContainerClient(new Uri(options.BlobContainerUri), new DefaultAzureCredential());
            }
            return new BlobContainerClient(options.ConnectionString, options.ContainerName);
        });

        services.AddLogging();
    })
    .Build();

host.Run();
