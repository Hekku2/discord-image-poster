@export()
type DiscordSettings = {
  token: string
  guildId: int
  channelId: int
}

@export()
type ImageStorageSettings = {
  blobContainerUri: string
  connectionString: string
  folderPath: string
}
