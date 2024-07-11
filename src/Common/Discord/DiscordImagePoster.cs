using Discord;
using Discord.Net;
using Discord.Rest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordImagePoster.Common.Discord;

public class DiscordImagePoster : IDiscordImagePoster, IDiscordCommandRegisterer
{
    private readonly ILogger<DiscordImagePoster> _logger;
    private readonly DiscordConfiguration _options;

    public DiscordImagePoster(ILogger<DiscordImagePoster> logger, IOptions<DiscordConfiguration> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task SendImageAsync(ImagePostingParameters parameters)
    {
        var file = new FileAttachment(parameters.ImageStream, parameters.FileName, parameters.Description, false, true);
        using var client = await GetAuthenticatedClient();

        var guild = await client.GetGuildAsync(_options.GuildId);
        var channel = await guild.GetChannelAsync(_options.ChannelId);

        var textChannel = await GetCorrectChannelAsync(client);
        if (textChannel == null)
        {
            _logger.LogError("Channel {ChannelId} not found or it was not text channel.", _options.ChannelId);
            return;
        }
        var sentMessage = await textChannel.SendFileAsync(file, parameters.Description ?? parameters.FileName, false);
    }

    public async Task RegisterCommandsAsync()
    {
        var guildCommand = new SlashCommandBuilder();
        guildCommand.WithName("post-random-image")
            .WithDescription("Post a random image to the channel.");

        try
        {
            using var client = await GetAuthenticatedClient();
            var guild = await client.GetGuildAsync(_options.GuildId);
            await guild.CreateApplicationCommandAsync(guildCommand.Build());
        }
        catch (HttpException ex)
        {
            _logger.LogError(ex, "Failed to create command.");
        }
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
