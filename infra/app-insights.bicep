@minLength(5)
param baseName string

@description('Location for all resources.')
param location string = resourceGroup().location

resource workspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: 'la-${baseName}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'ai-${baseName}'
  location: location
  kind: 'web'
  tags: {
    displayName: 'Application insights'
  }
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id
  }
}

output applicationInsightsName string = applicationInsights.name
output workspaceId string = workspace.id
