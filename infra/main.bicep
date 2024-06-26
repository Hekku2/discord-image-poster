import { DiscordSettings, ImageStorageSettings } from 'types.bicep'

@description('Base of the name for all resources.')
param baseName string

@description('Settings for Discord bot.')
param discordSettings DiscordSettings

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Web site package location. Leave empty if none is found.')
param webSitePackageLocation string = ''

@description('If true, messages are not sent to Discord. This should only be used when testing.')
param disableDiscordSending bool = false

module appInsights 'app-insights.bicep' = {
  name: 'application-insights'
  params: {
    baseName: baseName
    location: location
  }
}

resource functionAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: 'id-${baseName}'
  location: location
}

module keyVaultModule 'key-vault.bicep' = {
  name: 'key-vault'
  params: {
    location: location
    baseName: baseName
    secretUserRoleIdentities: [
      functionAppIdentity.name
    ]
    secrets: [
      {
        key: 'DiscordToken'
        value: discordSettings.token
      }
      {
        key: 'DiscordGuildId'
        value: '${discordSettings.guildId}'
      }
      {
        key: 'DiscordChannelId'
        value: '${discordSettings.channelId}'
      }
    ]
  }
}

module imageStorageModule 'image-storage.bicep' = {
  name: 'image-storage'
  params: {
    location: location
    baseName: baseName
    imageContainerName: 'images'
    imageReaderIdentities: [
      functionAppIdentity.name
    ]
  }
}

var imageSettings = {
  blobContainerUri: imageStorageModule.outputs.blobContainerUri
  folderPath: 'root'
}

module imageAnalyzerModule 'image-analyzer.bicep' = {
  name: 'image-analyzer'
  params: {
    location: location
    baseName: baseName
    cognitiveServiceUserIdentityNames: [
      functionAppIdentity.name
    ]
  }
}

module functions 'functions.bicep' = {
  name: 'functions'
  params: {
    applicationInsightsName: appInsights.outputs.applicationInsightsName
    location: location
    baseName: baseName
    keyVaultName: keyVaultModule.outputs.keyVaultName
    imageStorageSettings: imageSettings
    webSitePackageLocation: webSitePackageLocation
    disableDiscordSending: disableDiscordSending
    identityName: functionAppIdentity.name
    cognitiveServicesAccountName: imageAnalyzerModule.outputs.cognitiveServiceAccountName
  }
}
