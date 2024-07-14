using System.Net;
using DiscordImagePoster.FunctionApp.Isolated.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordImagePoster.FunctionApp.Isolated;

public class ImageSendFunction
{
    private readonly ILogger<ImageSendFunction> _logger;
    private readonly FeatureSettings _featureSettings;

    public ImageSendFunction(
        ILogger<ImageSendFunction> logger,
        IOptions<FeatureSettings> featureSettings
    )
    {
        _logger = logger;
        _featureSettings = featureSettings.Value;
    }

    [Function("SendImage")]
    public async Task<HttpResponseData> SendRandomImage(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellation)
    {
        _logger.LogInformation("Sending random image triggered manually.");

        await client.ScheduleNewOrchestrationInstanceAsync(nameof(ImageSendOrchestration), "", cancellation);

        return req.CreateResponse(HttpStatusCode.Accepted);
    }

    [Function("SendRandomImage")]
    public async Task TriggerTimerSendRandomImage(
        [TimerTrigger("0 0 */4 * * *")] TimerInfo timer,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellation)
    {
        _logger.LogDebug("Sending timed random image");
        if (_featureSettings.DisableTimedSending)
        {
            _logger.LogInformation("No timed sending enabled, skipping.");
            return;
        }

        await client.ScheduleNewOrchestrationInstanceAsync(nameof(ImageSendOrchestration), "", cancellation);

        if (timer.ScheduleStatus is not null)
        {
            _logger.LogDebug("Next timer schedule at: {next}", timer.ScheduleStatus.Next);
        }
    }
}
