import { DiscordSettings, ImageStorageSettings } from 'types.bicep'

@minLength(5)
param baseName string

@description('The name of the application insights to be used')
param applicationInsightsName string

@description('Settings for image storage.')
param imageStorageSettings ImageStorageSettings

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Web site package location. Leave empty if none is found.')
param webSitePackageLocation string = ''

@description('If true, messages are not sent to Discord. This should only be used when testing.')
param disableDiscordSending bool = false

@description('The name of the key vault that contains the secrets used by function app.')
param keyVaultName string

var hostingPlanName = 'asp-${baseName}'
var functionAppName = 'func-${baseName}'
var storageBlobDataOwnerRoleDefinitionId = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
  scope: resourceGroup()
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
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
      cors: {
        allowedOrigins: [
          'https://portal.azure.com'
        ]
      }
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
          name: 'FeatureSettings__DisableDiscordSending'
          value: '${disableDiscordSending}'
        }
        {
          name: '${discordSettingsKey}__Token'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=DiscordToken)'
        }
        {
          name: '${discordSettingsKey}__GuildId'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=DiscordGuildId)'
        }
        {
          name: '${discordSettingsKey}__ChannelId'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=DiscordChannelId)'
        }
        {
          name: '${blobStorageKey}__BlobContainerUri'
          value: imageStorageSettings.blobContainerUri
        }
        {
          name: '${blobStorageKey}__FolderPath'
          value: imageStorageSettings.folderPath
        }
        {
          name: '${imageIndexStorageKey}__BlobContainerUri'
          value: '${functionStorageAccount.properties.primaryEndpoints.blob}index'
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
output functionAppResourceId string = functionApp.id
