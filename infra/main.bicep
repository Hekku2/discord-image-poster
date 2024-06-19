import { DiscordSettings, ImageStorageSettings } from 'types.bicep'

@description('Base of the name for all resources.')
param baseName string

@description('Settings for Discord bot.')
param discordSettings DiscordSettings

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Web site package location. Leave empty if none is found.')
param webSitePackageLocation string = ''

module appInsights 'app-insights.bicep' = {
  name: 'application-insights'
  params: {
    baseName: baseName
    location: location
  }
}

// Storage account name must be between 3 and 24 characters in length and use numbers and lower-case letters only
resource imageStorage 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: substring(replace('stimages${baseName}', '-', ''), 0, 24)
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  tags: {
    displayName: 'Storage for function app'
  }
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    isHnsEnabled: true
  }
}

var imageSettings = {
  connectionString: 'DefaultEndpointsProtocol=https;AccountName=${imageStorage.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${imageStorage.listKeys().keys[0].value}'
  containerName: 'images'
  folderPath: 'root'
}

module functions 'functions.bicep' = {
  name: 'functions'
  params: {
    applicationInsightsName: appInsights.outputs.applicationInsightsName
    location: location
    baseName: baseName
    discordSettings: discordSettings
    imageStorageSettings: imageSettings
    webSitePackageLocation: webSitePackageLocation
  }
}
