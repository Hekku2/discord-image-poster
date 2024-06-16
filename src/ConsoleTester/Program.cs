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

    public static void Main(string[] args)
    {
        var containerClient = new BlobContainerClient("UseDevelopmentStorage=true", "images");

        var blobclient = containerClient.GetBlobClient("testfolder/test1.png");
        var prop = blobclient.GetProperties();
    }
}
