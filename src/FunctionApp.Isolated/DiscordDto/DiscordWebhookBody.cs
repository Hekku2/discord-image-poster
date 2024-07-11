namespace DiscordImagePoster.FunctionApp.Isolated.DiscordDto;

public class DiscordWebhookBody
{
    public required int Type { get; set; }
    public required DiscordServerMember Member { get; set; }
    public required DiscordInteractionData Data { get; set; }
}
