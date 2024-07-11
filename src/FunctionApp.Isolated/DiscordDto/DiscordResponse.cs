using System.Text.Json.Serialization;

namespace DiscordImagePoster.FunctionApp.Isolated.DiscordDto;

public class DiscordResponse
{
    [JsonPropertyName("type")]
    public required int Type { get; set; }
}
