using DiscordImagePoster.Common.BlobStorageImageService;
using DiscordImagePoster.Common.Discord;
using DiscordImagePoster.Common.ImageAnalysis;
using DiscordImagePoster.Common.IndexService;
using DiscordImagePoster.Common.RandomizationService;
using DiscordImagePoster.FunctionApp.Isolated;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Feature settings are used immediately because they are used to determine if the service should be disabled.
        var features = context.Configuration.GetSection(nameof(FeatureSettings)).Get<FeatureSettings>() ?? FeatureSettings.Default;
        services.AddOptions<FeatureSettings>().BindConfiguration(nameof(FeatureSettings)).ValidateDataAnnotations().ValidateOnStart();

        services.AddIndexServices();
        services.AddBlobStorageImageService();

        if (features.DisableDiscordSending)
            services.AddTransient<IDiscordImagePoster, NoOpDiscordImagePoster>();
        else
            services.AddDiscordSendingServices();

        if (features.DisableImageAnalysis)
            services.AddTransient<IImageAnalysisService, NoOpImageAnalysisService>();
        else
            services.AddImageAnalysisServices();

        services.AddTransient<IRandomizationService, RandomizationService>();

        services.AddLogging();
    })
    .Build();

host.Run();
