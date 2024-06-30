/*
 * Creates Cognitive Services Account for Computer Vision and gives access to the specified identities.
 */

@minLength(5)
param baseName string

param cognitiveServiceName string = 'aisa-${baseName}'

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Identities of which are assigned with Blob Data Reader to the containers.')
param cognitiveServiceUserIdentityNames string[]

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

module rbacAssingments 'image-analyzer-permissions.bicep' = [
  for (identityName, i) in cognitiveServiceUserIdentityNames: {
    name: 'cognitiveServiceUser-${i}'
    params: {
      cognitiveServiceName: cognitiveServiceAccount.name
      cognitiveServiceUserIdentityName: cognitiveServiceUserIdentityNames[0]
      cognitiveServiceUserIdentityResourceGroup: resourceGroup().name
    }
  }
]

output cognitiveServiceAccountName string = cognitiveServiceAccount.name
