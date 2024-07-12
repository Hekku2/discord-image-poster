using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

[DurableTask(nameof(ImageSendOrchestration))]
public class ImageSendOrchestration : TaskOrchestrator<string, string>
{
    public async override Task<string> RunAsync(TaskOrchestrationContext context, string input)
    {
        var logger = context.CreateReplaySafeLogger<ImageSendOrchestration>();

        // An extension method was generated for directly invoking "MyActivity".
        return await context.CallActivityAsync<string>(nameof(SendImageActivity), input);
    }
}
