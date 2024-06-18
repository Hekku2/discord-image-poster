using Azure.Storage.Blobs;
using DiscordImagePoster.Common.BlobStorageImageService;
using DiscordImagePoster.Common.Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddOptions<BlobStorageImageSourceOptions>().BindConfiguration(nameof(BlobStorageImageSourceOptions));
        services.AddOptions<DiscordConfiguration>().BindConfiguration(nameof(DiscordConfiguration));
        services.AddTransient<IDiscordImagePoster, DiscordImagePoster.Common.Discord.DiscordImagePoster>();
        services.AddTransient<IBlobStorageImageService, BlobStorageImageService>();
        services.AddTransient<BlobContainerClient>((serviceProvider) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<BlobStorageImageSourceOptions>>().Value;
            return new BlobContainerClient(options.ConnectionString, options.ContainerName);
        });
        services.AddLogging();
    })
    .Build();

host.Run();
