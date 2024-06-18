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


https://github.com/Azure/azure-quickstart-templates/blob/master/quickstarts/microsoft.app/container-app-acr/main.bicep
https://github.com/Azure/azure-functions-on-container-apps


 az acr manifest list --registry crhjnidiscordimageposters --name discord-image-poster --orderby time_desc --top 1

https://steijpwdowtvnz2.blob.core.windows.net/function-releases/20240618061416-35fd7285-189e-427e-afb5-377fbe8c988c.zip?sv=2018-03-28&sr=b&sig=YuJDFWQrbYdUbOf%2FeaB24MQ3vYZpZDe9Hu36XndSaLo%3D&st=2024-06-18T06%3A09%3A17Z&se=2034-06-18T06%3A14%3A17Z&sp=r
