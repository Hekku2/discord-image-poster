# Discord Image Poster
A bot which posts a random image to discord channel from Azrue Storage.

## Running locally

Preparation
 1. Create .env file for docker compose

```bash
docker compose build
docker compose up
```
Then call `http://localhost:8080/api/SendImage?code=mock-secret-for-local-testing`

## Outside documentation

Relevant parts of documentation

 * Azurite
   * [Authentication](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio%2Cblob-storage)
 * Discord.Net
   * [DiscordRestClient](https://docs.discordnet.dev/api/Discord.Rest.DiscordRestClient.html)
   * [UserExtensions.SendFileAsync](https://docs.discordnet.dev/api/Discord.UserExtensions.html#Discord_UserExtensions_SendFileAsync_Discord_IUser_Discord_FileAttachment_System_String_System_Boolean_Discord_Embed_Discord_RequestOptions_Discord_MessageComponent_Discord_Embed___)
