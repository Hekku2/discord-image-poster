using Discord;
using Discord.Rest;
using DiscordImagePoster.Common.Discord;
using DiscordImagePoster.Common.RandomizationService;
using DiscordImagePoster.FunctionApp.Isolated.DiscordDto;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DiscordImagePoster.FunctionApp.Isolated;

public class DiscordWebhookFunction
{
    private readonly ILogger<DiscordWebhookFunction> _logger;
    private readonly string _publicKey;
    private readonly IRandomImagePoster _randomImagePoster;

    public DiscordWebhookFunction(
        ILogger<DiscordWebhookFunction> logger,
        IOptions<DiscordConfiguration> options,
        IRandomImagePoster randomImagePoster
    )
    {
        _logger = logger;
        _publicKey = options.Value.PublicKey;
        _randomImagePoster = randomImagePoster;
    }

    [Function("HandleDiscordWebHook")]
    public async Task<HttpResponseData> HandleWebhook([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {

        _logger.LogInformation("Handling webhook.");
        var signature = req.GetFirstHeaderValue("X-Signature-Ed25519");
        if (signature is null)
        {
            _logger.LogError("No signature found.");
            return req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
        }
        var timestamp = req.GetFirstHeaderValue("X-Signature-Timestamp");
        if (timestamp is null)
        {
            _logger.LogError("No timestamp found.");
            return req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
        }

        var stringBody = await req.ReadAsStringAsync() ?? string.Empty;
        var client = new DiscordRestClient();
        var isValid = client.IsValidHttpInteraction(_publicKey, signature, timestamp, stringBody);
        if (!isValid)
        {
            _logger.LogError("Invalid signature.");
            return req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
        }

        _logger.LogInformation(stringBody);
        var response = JsonConvert.DeserializeObject<DiscordWebhookBody>(stringBody);
        switch (response?.Type)
        {
            case (int)InteractionType.Ping:
                _logger.LogInformation("Handle ping");
                return await CreatePongResponse(req);
            case (int)InteractionType.ApplicationCommand:
                _logger.LogInformation("Handle command");
                if (response?.Data?.Name == "post-random-image")
                {
                    await _randomImagePoster.PostRandomImageAsync();
                }
                return await CreateCommandResponse(req);
            default:
                _logger.LogInformation("Unknown command.");
                break;
        }
        // Default to command response for now
        return await CreateCommandResponse(req);
    }

    private static async Task<HttpResponseData> CreatePongResponse(HttpRequestData req)
    {
        var returnObject = new DiscordResponse
        {
            Type = (int)InteractionResponseType.Pong
        };
        var responseObject = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await responseObject.WriteAsJsonAsync(returnObject);
        return responseObject;
    }

    private static async Task<HttpResponseData> CreateCommandResponse(HttpRequestData req)
    {
        var returnObject = new DiscordResponse
        {
            Type = (int)InteractionResponseType.ChannelMessageWithSource
        };
        var responseObject = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await responseObject.WriteAsJsonAsync(returnObject);
        return responseObject;
    }

}

