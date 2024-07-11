namespace DiscordImagePoster.FunctionApp.Isolated.DiscordDto;

public class DiscordInteractionData
{
    public required int Type { get; set; }
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required dynamic[] Options { get; set; }
}
