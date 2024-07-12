using DiscordImagePoster.Common.RandomizationService;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

[DurableTask(nameof(SendImageActivity))]
public class SendImageActivity : TaskActivity<string, string>
{
    private readonly ILogger<SendImageActivity> _logger;
    private readonly IRandomImagePoster _randomImagePoster;

    public SendImageActivity(ILogger<SendImageActivity> logger, IRandomImagePoster randomImagePoster)
    {
        _logger = logger;
        _randomImagePoster = randomImagePoster;
    }

    public async override Task<string> RunAsync(TaskActivityContext context, string input)
    {
        await _randomImagePoster.PostRandomImageAsync();
        return "OK";
    }
}
