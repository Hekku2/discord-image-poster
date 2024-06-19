using Azure.Storage.Blobs;
using DiscordImagePoster.Common;
using DiscordImagePoster.Common.BlobStorageImageService;
using DiscordImagePoster.Common.Discord;
using DiscordImagePoster.Common.IndexService;
using DiscordImagePoster.FunctionApp.Isolated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddOptions<BlobStorageImageSourceOptions>().BindConfiguration(nameof(BlobStorageImageSourceOptions));
        services.AddOptions<DiscordConfiguration>().BindConfiguration(nameof(DiscordConfiguration));
        services.AddOptions<FeatureSettings>().BindConfiguration(nameof(FeatureSettings));
        services.AddOptions<ImageIndexOptions>().BindConfiguration(nameof(ImageIndexOptions));

        services.AddTransient<IDiscordImagePoster>(services =>
        {
            var options = services.GetRequiredService<IOptions<FeatureSettings>>().Value;
            return options.DisableDiscordSending ? new NoOpDiscordImagePoster() :
                new DiscordImagePoster.Common.Discord.DiscordImagePoster(
                services.GetRequiredService<ILogger<DiscordImagePoster.Common.Discord.DiscordImagePoster>>(),
                services.GetRequiredService<IOptions<DiscordConfiguration>>());
        });

        // TODO Check if keyed services can be used here
        services.AddTransient<IBlobStorageImageService, BlobStorageImageService>((services) =>
        {
            var options = services.GetRequiredService<IOptions<BlobStorageImageSourceOptions>>().Value;
            var containerClient = new BlobContainerClient(options.ConnectionString, options.ContainerName);
            return new BlobStorageImageService(
                services.GetRequiredService<ILogger<BlobStorageImageService>>(),
                services.GetRequiredService<IOptions<BlobStorageImageSourceOptions>>(),
                containerClient);
        });
        services.AddTransient<IIndexService, BlobStorageIndexService>(services =>
        {
            var options = services.GetRequiredService<IOptions<ImageIndexOptions>>().Value;
            var containerClient = new BlobContainerClient(options.ConnectionString, options.ContainerName);
            return new BlobStorageIndexService(
                services.GetRequiredService<ILogger<BlobStorageIndexService>>(),
                containerClient);
        });
        services.AddLogging();
    })
    .Build();

host.Run();
