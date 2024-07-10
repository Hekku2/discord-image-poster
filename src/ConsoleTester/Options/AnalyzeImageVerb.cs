using CommandLine;

namespace DiscordImagePoster.ConsoleTester.Options;

[Verb("analyze-image", HelpText = "Analyze an image.")]
public class AnalyzeImageVerb
{
    [Option('i', "image", Required = true, HelpText = "Path to the image file to analyze.")]
    public required string ImagePath { get; set; }

    [Option('o', "output", Required = true, HelpText = "Path to save the output.")]
    public required string OutputPath { get; set; }

}
