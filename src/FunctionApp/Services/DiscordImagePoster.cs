using Discord;
using Discord.Rest;
using FunctionApp.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunctionApp.Services;

public interface IDiscordImagePoster
{
    Task SendImage(Stream stream, string fileName);
}

public class DiscordImagePoster : IDiscordImagePoster
{
    private readonly ILogger<DiscordImagePoster> _logger;
    private readonly DiscordOptions _options;

    public DiscordImagePoster(ILogger<DiscordImagePoster> logger, IOptions<DiscordOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task SendImage(Stream stream, string fileName)
    {
        var file = new FileAttachment(stream, fileName, "File file true", false, true);
        var embed = new EmbedBuilder { ImageUrl = $"attachment://{fileName}" }.Build();

        using var client = await GetAuthenticatedClient();

        var guild = await client.GetGuildAsync(_options.GuildId);
        var channel = await guild.GetChannelAsync(_options.ChannelId);

        var textChannel = await GetCorrectChannelAsync(client);
        if (textChannel == null)
        {
            _logger.LogError("Channel not found");
            return;
        }
        var sentMessage = await textChannel.SendFileAsync(file, null, false, embed: embed);
    }

    private async Task<DiscordRestClient> GetAuthenticatedClient()
    {
        var client = new DiscordRestClient();
        await client.LoginAsync(TokenType.Bot, _options.Token, true);

        return client;
    }

    private async Task<ITextChannel?> GetCorrectChannelAsync(DiscordRestClient client)
    {
        var guild = await client.GetGuildAsync(_options.GuildId);
        var channel = await guild.GetChannelAsync(_options.ChannelId);

        return channel as ITextChannel;
    }
}
