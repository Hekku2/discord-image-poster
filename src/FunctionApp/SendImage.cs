using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure;
using FunctionApp.Services;

namespace FunctionApp;

public class SendImage
{
    private readonly ILogger _logger;
    private readonly IDiscordImagePoster _discordImagePoster;
    private readonly IImageService _imageService;

    public SendImage(ILogger<SendImage> logger, IDiscordImagePoster discordImagePoster, IImageService imageService)
    {
        _logger = logger;
        _discordImagePoster = discordImagePoster;
        _imageService = imageService;
    }

    [Function("SendImage")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var image = _imageService.GetRandomImageStream();
        if (image == null || image.Result == null)
        {
            _logger.LogError("No image found");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }
        await _discordImagePoster.SendImage(image.Result.Content, "test1.png");

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
