@minLength(5)
param baseName string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Identities of which are assigned with Blob Data Reader to the containers.')
param cognitiveServiceUserIdentityNames string[]

var cognitiveServicesUserRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'a97b65f3-24c7-4388-baec-2e87135dc908'
)

resource identities 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' existing = [
  for identityName in cognitiveServiceUserIdentityNames: {
    name: identityName
    scope: resourceGroup()
  }
]

var cognitiveServiceName = 'aisa-${baseName}'
resource cognitiveServiceAccount 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: cognitiveServiceName
  location: location
  kind: 'ComputerVision'
  identity: {
    type: 'SystemAssigned'
  }
  sku: {
    name: 'F0'
  }
  properties: {
    publicNetworkAccess: 'Enabled'
    customSubDomainName: cognitiveServiceName
  }
}

resource cognitiveServiceAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for (identityName, i) in cognitiveServiceUserIdentityNames: {
    scope: cognitiveServiceAccount
    name: guid(identities[i].id, cognitiveServicesUserRoleDefinitionId, cognitiveServiceAccount.id)
    properties: {
      principalId: identities[i].properties.principalId
      principalType: 'ServicePrincipal'
      roleDefinitionId: cognitiveServicesUserRoleDefinitionId
    }
  }
]

output cognitiveServiceAccountName string = cognitiveServiceAccount.name
