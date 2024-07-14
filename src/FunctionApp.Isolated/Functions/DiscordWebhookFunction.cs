using Discord;
using Discord.Rest;
using DiscordImagePoster.Common.Discord;
using DiscordImagePoster.Common.RandomizationService;
using DiscordImagePoster.FunctionApp.Isolated.DiscordDto;
using DiscordImagePoster.FunctionApp.Isolated.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DiscordImagePoster.FunctionApp.Isolated;

public class DiscordWebhookFunction
{
    private readonly ILogger<DiscordWebhookFunction> _logger;
    private readonly string _publicKey;

    public DiscordWebhookFunction(
        ILogger<DiscordWebhookFunction> logger,
        IOptions<DiscordConfiguration> options
    )
    {
        _logger = logger;
        _publicKey = options.Value.PublicKey;
    }

    [Function("HandleDiscordWebHook")]
    public async Task<HttpResponseData> HandleWebhook(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellation
        )
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
        var isValid = new DiscordRestClient().IsValidHttpInteraction(_publicKey, signature, timestamp, stringBody);
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
                    await client.ScheduleNewOrchestrationInstanceAsync(nameof(ImageSendOrchestration), "", cancellation);
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

