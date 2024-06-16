using Azure.Storage.Blobs;
using FunctionApp.Options;
using FunctionApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddOptions<BlobStorageImageSourceOptions>().BindConfiguration(nameof(BlobStorageImageSourceOptions));
        services.AddOptions<DiscordOptions>().BindConfiguration(nameof(DiscordOptions));
        services.AddTransient<IDiscordImagePoster, DiscordImagePoster>();
        services.AddTransient<IImageService, ImageService>();
        services.AddTransient<BlobContainerClient>((serviceProvider) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<BlobStorageImageSourceOptions>>().Value;
            return new BlobContainerClient(options.ConnectionString, options.ContainerName);
        });
        services.AddLogging();
    })
    .Build();

host.Run();
