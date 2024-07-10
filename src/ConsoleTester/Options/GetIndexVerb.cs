using CommandLine;

namespace DiscordImagePoster.ConsoleTester.Options;

[Verb("get-index", HelpText = "Get the image index and save it to a file.")]
public class GetIndexVerb
{
    [Option('o', "output", Required = true, HelpText = "Path to save the index file.")]
    public required string OutputPath { get; set; }
}
