using System.Text.Json;
using CommandLine;
using DiscordImagePoster.Common.BlobStorageImageService;
using DiscordImagePoster.Common.Discord;
using DiscordImagePoster.Common.IndexService;
using DiscordImagePoster.Common.ImageAnalysis;
using DiscordImagePoster.ConsoleTester.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscordImagePoster.ConsoleTester;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Note: CreateApplicationBuilder is mainly used for easier access to config, DI, etc.
        var builder = Host.CreateApplicationBuilder(args);

        await Parser.Default.ParseArguments<DiscordSendVerb, GetIndexVerb, RefreshIndexVerb, AnalyzeImageVerb, RegisterCommandVerb>(args)
          .MapResult(
            async (DiscordSendVerb options) => await SendImageToDiscord(builder, options),
            async (GetIndexVerb options) => await GetIndex(builder, options),
            async (RefreshIndexVerb options) => await RefreshIndex(builder, options),
            async (AnalyzeImageVerb options) => await AnalyzeImage(builder, options),
            async (RegisterCommandVerb options) => await RegisterCommands(builder, options),
            async _ => await Task.CompletedTask);
    }

    private static async Task AnalyzeImage(HostApplicationBuilder builder, AnalyzeImageVerb verb)
    {
        builder.Services.AddImageAnalysisServices();
        using var stream = File.OpenRead(verb.ImagePath);
        var binaryData = await BinaryData.FromStreamAsync(stream);

        var host = builder.Build();

        var result = await host.Services.GetRequiredService<IImageAnalysisService>().AnalyzeImageAsync(binaryData);
        var serialized = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(verb.OutputPath, serialized);
    }

    public static async Task SendImageToDiscord(HostApplicationBuilder builder, DiscordSendVerb verb)
    {
        builder.Services.AddDiscordSendingServices();

        if (verb.Analyze)
        {
            builder.Services.AddImageAnalysisServices();
        }

        var host = builder.Build();

        using var stream = File.OpenRead(verb.ImagePath);
        var binaryData = await BinaryData.FromStreamAsync(stream);
        if (verb.Analyze)
        {
            await host.Services.GetRequiredService<IImageAnalysisService>().AnalyzeImageAsync(binaryData);
        }

        await host.Services.GetRequiredService<IDiscordImagePoster>().SendImageAsync(new ImagePostingParameters
        {
            ImageStream = binaryData.ToStream(),
            FileName = Path.GetFileName(verb.ImagePath),
            Description = verb.Description
        });
    }

    public static async Task GetIndex(HostApplicationBuilder builder, GetIndexVerb verb)
    {
        builder.Services.AddIndexServices();
        builder.Services.AddBlobStorageImageService();
        var host = builder.Build();

        var index = await host.Services.GetRequiredService<IIndexService>().GetIndexAsync();
        var serialized = JsonSerializer.Serialize(index, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(verb.OutputPath, serialized);
    }

    public static async Task RefreshIndex(HostApplicationBuilder builder, RefreshIndexVerb verb)
    {
        builder.Services.AddIndexServices();
        builder.Services.AddBlobStorageImageService();
        var host = builder.Build();

        await host.Services.GetRequiredService<IIndexService>().RefreshIndexAsync();
    }

    public static async Task RegisterCommands(HostApplicationBuilder builder, RegisterCommandVerb verb)
    {
        builder.Services.AddDiscordSendingServices();
        var host = builder.Build();

        await host.Services.GetRequiredService<IDiscordCommandRegisterer>().RegisterCommandsAsync();
    }
}
