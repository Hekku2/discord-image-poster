# Discord Image Poster

A bot which posts periodically a random image to discord channel from Azure
Storage.

Currently this is designed to work with one server and one channel.

## Deployment and running

Following section describes what needs be done to deploy and run this
application. Instructions are still WIP, proper documentation and scripts can
be provided later.

This service can be run in following ways

1. Local, without container. (Azure Core Tools, `func start`)
2. Local, with container (docker compose)
3. From Azure (the intended way for production)

### Configuration

Discord

  1. Create application in [Discord portal](https://discord.com/developers/applications).
  2. [Installing Discord Bot to Discord Server](README.md#installing-discord-bot-to-discord-server)
  3. Get Token from Discord Portal (Check the bot page)
  4. Get Guild Id (server ID) and Channel Id from Discord server. Easies way to is
  to navigate to Discord server with browser and take those ids from browser url,
  such as `https://discord.com/channels/123123/666666`

#### Installing Discord Bot to Discord Server

Not sure if there is an easier way, but the current way is to call following url:
`https://discord.com/oauth2/authorize?client_id=<bot id here>&permissions=2048&scope=bot`

  * Permission 2048 is "Send Messages"
  * Other combinations can be checked from `https://discord.com/developers/applications/<id here>/bot`

### Running locally with Azure Core Tools

This requires Azurite or proper Azure Storage to function correctly.

Settings are read from `local.settings.json` which needs to be generated.

Example:

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "DiscordOptions__Token": "token here",
        "DiscordOptions__GuildId": "server id here",
        "DiscordOptions__ChannelId": "channel id here",
        "BlobStorageImageSourceOptions__ConnectionString": "DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://local.storage.emulator:10000/devstoreaccount1;QueueEndpoint=http://local.storage.emulator:10001/devstoreaccount1;",
        "BlobStorageImageSourceOptions__ContainerName": "images",
        "BlobStorageImageSourceOptions__FolderPath": "testfolder"
    }
}

```

To start

```bash
cd ./src/FunctionApp.Isolated/
func start
```

### Running locally in container

Preparation
 1. Create `developer-settings.json` (based on developer-settings-sample.json)
 1. Create .env file for docker compose

```bash
docker compose build
docker compose up
```

NOTE: For some reason, timer doesn't seem start correctly on the first time, but it seems to after retry.
`Microsoft.Azure.WebJobs.Host.Listeners.FunctionListenerException: The listener for function 'Functions.SendRandomImage' was unable to start`

Then call `http://localhost:8080/api/SendImage?code=mock-secret-for-local-testing`

### Running in Azure

Preparation
 1. Create `developer-settings.json` (based on developer-settings-sample.json)

```bash
./Create-Environment.ps1
./Publish-App.ps1
```

Then get the url from Azure Portal etc.

## Outside documentation

Relevant parts of documentation

 * Azurite
   * [Authentication](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio%2Cblob-storage)
 * Discord.Net
   * [DiscordRestClient](https://docs.discordnet.dev/api/Discord.Rest.DiscordRestClient.html)
   * [UserExtensions.SendFileAsync](https://docs.discordnet.dev/api/Discord.UserExtensions.html#Discord_UserExtensions_SendFileAsync_Discord_IUser_Discord_FileAttachment_System_String_System_Boolean_Discord_Embed_Discord_RequestOptions_Discord_MessageComponent_Discord_Embed___)
