import { CognitiveServiceSettings } from 'types.bicep'

/*
 * Either creates a new CognitiveService account or uses an existing one.
 * and adds the necessary permissions to the user identity.
 * This is seprate module because the existing CognitiveService account
 * may be in a different resource group (scope).
 */

@minLength(5)
param baseName string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Settings for deciding which CognitiveService resource is used.')
param cognitiveService CognitiveServiceSettings

@description('Identities of which are assigned with Blob Data Reader to the containers.')
param cognitiveServiceUserIdentityName string

// NOTE: ?? ''  - is used to circument type warnings.
var existingServiceResourceGroupName = cognitiveService.existingServiceResourceGroup ?? ''
var getExisting = cognitiveService.existingServiceName != null

resource old 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = if (getExisting) {
  name: cognitiveService.existingServiceName ?? ''
  scope: resourceGroup(existingServiceResourceGroupName)
}

module rbacAssingments 'image-analyzer-permissions.bicep' = if (getExisting) {
  scope: resourceGroup(existingServiceResourceGroupName)
  name: 'cognitiveServiceUser-old'
  params: {
    cognitiveServiceName: old.name
    cognitiveServiceUserIdentityName: cognitiveServiceUserIdentityName
    cognitiveServiceUserIdentityResourceGroup: resourceGroup().name
  }
}

module new 'image-analyzer.bicep' = if (!getExisting) {
  name: 'newAnalyzer'
  params: {
    baseName: baseName
    location: location
    cognitiveServiceUserIdentityNames: [cognitiveServiceUserIdentityName]
  }
}

output cognitiveServiceAccountName string = getExisting ? old.name : new.outputs.cognitiveServiceAccountName
output cognitiveServiceResourceGroup string = getExisting ? existingServiceResourceGroupName : resourceGroup().name
