using System.Net;
using DiscordImagePoster.Common.RandomizationService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordImagePoster.FunctionApp.Isolated;

public class ImageSendFunction
{
    private readonly ILogger<ImageSendFunction> _logger;
    private readonly FeatureSettings _featureSettings;
    private readonly IRandomImagePoster _randomImagePoster;

    public ImageSendFunction(
        ILogger<ImageSendFunction> logger,
        IOptions<FeatureSettings> featureSettings,
        IRandomImagePoster randomImagePoster
    )
    {
        _logger = logger;
        _featureSettings = featureSettings.Value;
        _randomImagePoster = randomImagePoster;
    }

    [Function("SendImage")]
    public async Task<HttpResponseData> SendRandomImage([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("Sending random image triggered manually.");

        await _randomImagePoster.PostRandomImageAsync();

        return req.CreateResponse(HttpStatusCode.Accepted);
    }

    [Function("SendRandomImage")]
    public async Task TriggerTimerSendRandomImage([TimerTrigger("0 0 */4 * * *")] TimerInfo timer)
    {
        _logger.LogDebug("Sending timed random image");
        if (_featureSettings.DisableTimedSending)
        {
            _logger.LogInformation("No timed sending enabled, skipping.");
            return;
        }

        await _randomImagePoster.PostRandomImageAsync();

        if (timer.ScheduleStatus is not null)
        {
            _logger.LogDebug("Next timer schedule at: {next}", timer.ScheduleStatus.Next);
        }
    }
}
