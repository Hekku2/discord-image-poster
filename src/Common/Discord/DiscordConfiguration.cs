using System.ComponentModel.DataAnnotations;

namespace DiscordImagePoster.Common.Discord;

/// <summary>
/// Represents the configuration for the Discord bot that is required for
/// connecting to the Discord API.
/// </summary>
public class DiscordConfiguration
{
    /// <summary>
    /// The token of the bot. This is used to authenticate the bot with the
    /// Discord API.
    /// </summary>
    [Required]
    [MinLength(1)]
    public required string Token { get; set; }

    /// <summary>
    /// The ID of the guild / server where the bot will post images.
    /// This can be fetched from the Discord server url.
    /// For example, the ID of the guild in the url https://discord.com/channels/123123/666666 is 123123.
    /// </summary>
    [Required]
    public ulong GuildId { get; set; }

    /// <summary>
    /// The ID of the channel where the bot will post images.
    /// This can be fetched from the Discord server url.
    /// For example, the ID of the guild in the url https://discord.com/channels/123123/666666 is 666666.
    /// </summary>
    [Required]
    public ulong ChannelId { get; set; }

    /// <summary>
    /// The public key of the bot. This is used to verify the authenticity of
    /// the requests sent to the bot.
    /// </summary>
    [Required]
    public required string PublicKey { get; set; }
}
