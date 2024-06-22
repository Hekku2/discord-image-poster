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

var imageContainerName = 'images'
resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: imageStorage
  name: 'default'
}

resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobServices
  name: imageContainerName
}

var imageSettings = {
  connectionString: 'DefaultEndpointsProtocol=https;AccountName=${imageStorage.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${imageStorage.listKeys().keys[0].value}'
  blobContainerUri: '${imageStorage.properties.primaryEndpoints.blob}/${imageContainerName}'
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
    disableDiscordSending: disableDiscordSending
  }
}

// TODO refactor this. this should only require reading permission. Image INDEX requires more permissions and currently these are in same place
// TODO Also this assignment could probably be a separate module etc.
var storageBlobDataOwnerRoleDefinitionId = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
resource functionAppFunctionBlobStorageAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: imageStorage
  name: guid(functions.name, storageBlobDataOwnerRoleDefinitionId, imageStorage.id)
  properties: {
    principalId: functions.outputs.functionAppPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      storageBlobDataOwnerRoleDefinitionId
    )
  }
}
