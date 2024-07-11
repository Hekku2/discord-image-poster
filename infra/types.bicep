@export()
type DiscordSettings = {
  @secure()
  token: string
  guildId: int
  channelId: int
  publicKey: string
}

@export()
type ImageStorageSettings = {
  blobContainerUri: string
  folderPath: string
}

@export()
type SecretKeyValue = {
  @description('The name of the secret')
  key: string

  @description('The value of the secret')
  @secure()
  value: string
}

@description('Settings for deciding which CognitiveService resource is used.')
@export()
type CognitiveServiceSettings = {
  @description('The name of the CognitiveService resource. If null, a new resource is created.')
  existingServiceName: string?

  @description('The resource group of the existing CognitiveService resource. If null, current resource group is used.')
  existingServiceResourceGroup: string?
}
