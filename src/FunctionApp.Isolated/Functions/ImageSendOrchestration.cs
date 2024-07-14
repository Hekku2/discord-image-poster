using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace DiscordImagePoster.FunctionApp.Isolated.Functions;

public static class ImageSendOrchestration
{

    [Function(nameof(ImageSendOrchestration))]
    public static async Task<string> RunAsync([OrchestrationTrigger] TaskOrchestrationContext context, string input)
    {
        var logger = context.CreateReplaySafeLogger(nameof(ImageSendOrchestration));
        logger.LogInformation("Starting image send orchestration.");

        return await context.CallActivityAsync<string>(nameof(SendRandomImageActivity), input);

    }
}
