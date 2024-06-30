/*
 * Create a role assignment to assign the Cognitive Services User role to the given User Assigned Identities.
 * This should be deployed to the resource group where Cognitive Services Account is. Identities can be in
 * a different resource group.
 */

@description('Name of the Cognitive Services account.')
param cognitiveServiceName string

@description('Identities of which are assigned with Blob Data Reader to the containers.')
param cognitiveServiceUserIdentityName string
param cognitiveServiceUserIdentityResourceGroup string

var cognitiveServicesUserRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'a97b65f3-24c7-4388-baec-2e87135dc908'
)

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' existing = {
  name: cognitiveServiceUserIdentityName
  scope: resourceGroup(cognitiveServiceUserIdentityResourceGroup)
}

resource cognitiveServiceAccount 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: cognitiveServiceName
  scope: resourceGroup()
}

resource cognitiveServiceAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: cognitiveServiceAccount
  name: guid(identity.id, cognitiveServicesUserRoleDefinitionId, cognitiveServiceAccount.id)
  properties: {
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: cognitiveServicesUserRoleDefinitionId
  }
}

output cognitiveServiceAccountName string = cognitiveServiceAccount.name
