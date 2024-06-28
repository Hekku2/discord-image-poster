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

@description('The name of the identity that will be used by the function app.')
param identityName string

@description('Cognitive services account name.')
param cognitiveServicesAccountName string

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

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' existing = {
  name: identityName
  scope: resourceGroup()
}

resource cognitiveServiceAccount 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: cognitiveServicesAccountName
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
var imageAnalysisKey = 'ImageAnalysisConfiguration'

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  kind: 'linux,functionapp'
  name: functionAppName
  location: location
  tags: {
    displayName: 'Function app'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  properties: {
    reserved: true
    serverFarmId: hostingPlan.id
    httpsOnly: true
    keyVaultReferenceIdentity: identity.id
    siteConfig: {
      defaultDocuments: []
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      phpVersion: null
      use32BitWorkerProcess: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      cors: {
        allowedOrigins: [
          'https://portal.azure.com'
        ]
      }
      keyVaultReferenceIdentity: identity.id
      appSettings: [
        {
          name: 'AzureWebJobsStorage__accountName'
          value: functionStorageAccount.name
        }
        {
          name: 'AzureWebJobsStorage__clientId'
          value: identity.properties.clientId
        }
        {
          name: 'AzureWebJobsStorage__credential'
          value: 'managedidentity'
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
          name: 'WEBSITE_RUN_FROM_PACKAGE_BLOB_MI_RESOURCE_ID'
          value: identity.id
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: identity.properties.clientId
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
        {
          name: '${imageAnalysisKey}__Endpoint'
          value: cognitiveServiceAccount.properties.endpoint
        }
      ]
    }
  }
}

resource functionAppFunctionBlobStorageAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: functionStorageAccount
  name: guid(identity.id, storageBlobDataOwnerRoleDefinitionId, functionStorageAccount.id)
  properties: {
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      storageBlobDataOwnerRoleDefinitionId
    )
  }
}

output functionAppResourceId string = functionApp.id
