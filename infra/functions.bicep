import { DiscordSettings, ImageStorageSettings } from 'types.bicep'

@minLength(5)
param baseName string

@description('The name of the application insights to be used')
param applicationInsightsName string

@description('Settings for Discord bot.')
param discordSettings DiscordSettings

@description('Settings for image storage.')
param imageStorageSettings ImageStorageSettings

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Web site package location. Leave empty if none is found.')
param webSitePackageLocation string = ''

var hostingPlanName = 'asp-${baseName}'
var functionAppName = 'func-${baseName}'
var storageBlobDataOwnerRoleDefinitionId = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
  scope: resourceGroup()
}

resource functionStorageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: 'st${uniqueString(resourceGroup().id)}'
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

resource hostingPlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: hostingPlanName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  kind: 'functionapp,linux'
  properties: {
    reserved: true
  }
  tags: {
    displayName: 'Server for function app'
  }
}

var discordSettingsKey = 'DiscordConfiguration'
var blobStorageKey = 'BlobStorageImageSourceOptions'
var imageIndexStorageKey = 'ImageIndexOptions'

resource functionApp 'Microsoft.Web/sites@2021-02-01' = {
  kind: 'linux,functionapp'
  name: functionAppName
  location: location
  tags: {
    displayName: 'Function app'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    reserved: true
    serverFarmId: hostingPlan.id
    httpsOnly: true
    siteConfig: {
      defaultDocuments: []
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      phpVersion: null
      use32BitWorkerProcess: false
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'

      appSettings: [
        {
          name: 'AzureWebJobsStorage__accountName'
          value: functionStorageAccount.name
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'WEBSITE_MOUNT_ENABLED'
          value: '1'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: webSitePackageLocation
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: reference(applicationInsights.id, '2015-05-01').InstrumentationKey
        }
        {
          name: '${discordSettingsKey}__Token'
          value: discordSettings.token
        }
        {
          name: '${discordSettingsKey}__GuildId'
          value: '${discordSettings.guildId}'
        }
        {
          name: '${discordSettingsKey}__ChannelId'
          value: '${discordSettings.channelId}'
        }
        {
          name: '${blobStorageKey}__ConnectionString'
          value: imageStorageSettings.connectionString
        }
        {
          name: '${blobStorageKey}__ContainerName'
          value: imageStorageSettings.containerName
        }
        {
          name: '${blobStorageKey}__FolderPath'
          value: imageStorageSettings.folderPath
        }
        {
          name: '${imageIndexStorageKey}__ConnectionString'
          value: imageStorageSettings.connectionString
        }
        {
          name: '${imageIndexStorageKey}__ContainerName'
          value: 'index'
        }
      ]
    }
  }
}

resource functionAppFunctionBlobStorageAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: functionStorageAccount
  name: guid(functionApp.id, storageBlobDataOwnerRoleDefinitionId, functionStorageAccount.id)
  properties: {
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      storageBlobDataOwnerRoleDefinitionId
    )
  }
}

output functionAppPrincipalId string = functionApp.identity.principalId
