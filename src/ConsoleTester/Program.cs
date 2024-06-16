using Discord;
using Discord.Rest;
using System.Net;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure;


namespace DiscordImagePoster.ConsoleTester;

public class Program
{

    public static async Task Main(string[] args)
    {
        var containerClient = new BlobContainerClient("UseDevelopmentStorage=true", "images");

        //BlobServiceClient client = new(new Uri("https:// local.storage.emulator:10000/devstoreaccount1"), new DefaultAzureCredential());
        //var containerClient = client.GetBlobContainerClient("images");

        var blobName = "testfolder/test1.png";

        var blobclient = containerClient.GetBlobClient("testfolder/test1.png");
        var prop = blobclient.GetProperties();
    }
}
