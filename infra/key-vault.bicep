import { SecretKeyValue } from 'types.bicep'

@description('Base of the name for all resources.')
param baseName string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Secrets to store in the key vault.')
param secrets SecretKeyValue[]

@description('Secret users to grant access to the key vault.')
param secretUserRoleIdentities string[]

var secretUserRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '4633458b-17de-408a-b874-0445c86b69e6'
)

resource identities 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' existing = [
  for identityName in secretUserRoleIdentities: {
    name: identityName
    scope: resourceGroup()
  }
]

var keyVaultName = 'kv${uniqueString(resourceGroup().id, baseName)}'
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    enableRbacAuthorization: true
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    tenantId: tenant().tenantId
    enableSoftDelete: false
    accessPolicies: []
    sku: {
      name: 'standard'
      family: 'A'
    }
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

resource keyVaultSecrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = [
  for secret in secrets: {
    parent: keyVault
    name: secret.key
    properties: {
      value: secret.value
    }
  }
]

resource secretUserRoleAssingment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for (identityName, i) in secretUserRoleIdentities: {
    scope: keyVault
    name: guid(identities[i].id, secretUserRoleDefinitionId, keyVault.id)
    properties: {
      principalId: identities[i].properties.principalId
      principalType: 'ServicePrincipal'
      roleDefinitionId: secretUserRoleDefinitionId
    }
  }
]

output keyVaultName string = keyVaultName
