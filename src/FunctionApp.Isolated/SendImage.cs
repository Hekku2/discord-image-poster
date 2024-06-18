using System.Net;
using DiscordImagePoster.Common.BlobStorageImageService;
using DiscordImagePoster.Common.Discord;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DiscordImagePoster.FunctionApp.Isolated;

public class SendImage
{
    private readonly ILogger _logger;
    private readonly IDiscordImagePoster _discordImagePoster;
    private readonly IBlobStorageImageService _imageService;

    public SendImage(ILogger<SendImage> logger, IDiscordImagePoster discordImagePoster, IBlobStorageImageService imageService)
    {
        _logger = logger;
        _discordImagePoster = discordImagePoster;
        _imageService = imageService;
    }

    [Function("SendImage")]
    public async Task<HttpResponseData> HttpTriggerSendRandomImage([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("Sending random image triggered manually.");

        await SendRandomImage();

        return req.CreateResponse(HttpStatusCode.Accepted);
    }

    [Function("SendRandomImage")]
    public async Task TriggerTimerSendRandomImage([TimerTrigger("0 0 */4 * * *")] TimerInfo timer)
    {
        _logger.LogDebug("Sending timed random image");
        await SendRandomImage();

        if (timer.ScheduleStatus is not null)
        {
            _logger.LogDebug("Next timer schedule at: {next}", timer.ScheduleStatus.Next);
        }
    }

    private async Task SendRandomImage()
    {
        var result = await _imageService.GetRandomImageStream();
        if (result is null)
        {
            _logger.LogError("No image found");
            return;
        }
        await _discordImagePoster.SendImage(result.Value.Item2.Content, result.Value.Item1);
    }
}
