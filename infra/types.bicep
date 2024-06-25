@export()
type DiscordSettings = {
  @secure()
  token: string
  guildId: int
  channelId: int
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
