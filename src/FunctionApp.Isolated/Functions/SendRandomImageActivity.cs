using DiscordImagePoster.Common.RandomizationService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

public class SendRandomImageActivity
{
    private readonly ILogger<SendRandomImageActivity> _logger;
    private readonly IRandomImagePoster _randomImagePoster;

    public SendRandomImageActivity(ILogger<SendRandomImageActivity> logger, IRandomImagePoster randomImagePoster)
    {
        _logger = logger;
        _randomImagePoster = randomImagePoster;
    }

    [Function(nameof(SendRandomImageActivity))]
    public async Task RunAsync([ActivityTrigger] string context)
    {
        _logger.LogInformation("Sending random image triggered by orchestration.");
        await _randomImagePoster.PostRandomImageAsync();
    }
}
