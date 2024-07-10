using CommandLine;

namespace DiscordImagePoster.ConsoleTester.Options;

[Verb("discord-send", HelpText = "Send an image to a Discord channel.")]
public class DiscordSendVerb
{
    [Option('i', "image", Required = true, HelpText = "Path to the image file to send.")]
    public required string ImagePath { get; set; }

    [Option('d', "description", Required = false, HelpText = "Description of the image.")]
    public string? Description { get; set; }

    [Option('a', "analyze", Required = false, HelpText = "Analyze the image before sending. If this is used, analyzator settings are mandatory.")]
    public bool Analyze { get; set; }
}
