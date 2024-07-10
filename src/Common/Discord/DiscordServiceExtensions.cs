using Microsoft.Extensions.DependencyInjection;

namespace DiscordImagePoster.Common.Discord;

public static class DiscordServiceExtensions
{
    /// <summary>
    /// Register the Discord sending service..
    /// </summary>
    /// <param name="hostbuilder">HostBuilder</param>
    /// <returns>Hostbuilder</returns>
    public static IServiceCollection AddDiscordSendingServices(this IServiceCollection services)
    {
        services.AddOptions<DiscordConfiguration>().BindConfiguration(nameof(DiscordConfiguration)).ValidateDataAnnotations().ValidateOnStart();
        services.AddTransient<IDiscordImagePoster, DiscordImagePoster>();
        return services;
    }
}
