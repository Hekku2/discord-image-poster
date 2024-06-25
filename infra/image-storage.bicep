@minLength(5)
param baseName string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Names of the container to create in the storage account.')
param imageContainerName string

@description('Identities of which are assigned with Blob Data Reader to the containers.')
param imageReaderIdentities string[]

resource identities 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' existing = [
  for identityName in imageReaderIdentities: {
    name: identityName
    scope: resourceGroup()
  }
]

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

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: imageStorage
  name: 'default'
}

resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobServices
  name: imageContainerName
}

var storageBlobDataReaderRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
)

resource imageRearderAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for (identityName, i) in imageReaderIdentities: {
    scope: container
    name: guid(identities[i].id, storageBlobDataReaderRoleDefinitionId, container.id)
    properties: {
      principalId: identities[i].properties.principalId
      principalType: 'ServicePrincipal'
      roleDefinitionId: storageBlobDataReaderRoleDefinitionId
    }
  }
]

output blobContainerUri string = '${imageStorage.properties.primaryEndpoints.blob}${imageContainerName}'
