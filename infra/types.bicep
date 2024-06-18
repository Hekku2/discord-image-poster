@export()
type DiscordSettings = {
  token: string
  guildId: int
  channelId: int
}

@export()
type ImageStorageSettings = {
  connectionString: string
  containerName: string
  folderPath: string
}
