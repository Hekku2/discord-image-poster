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
  blobContainerUri: '${imageStorage.properties.primaryEndpoints.blob}${imageContainerName}'
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

// TODO Also this assignment could probably be a separate module etc.
var storageBlobDataReaderRoleDefinitionId = '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
resource functionAppFunctionBlobStorageAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: container
  name: guid(functions.name, storageBlobDataReaderRoleDefinitionId, container.id)
  properties: {
    principalId: functions.outputs.functionAppPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      storageBlobDataReaderRoleDefinitionId
    )
  }
}
