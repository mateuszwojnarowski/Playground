@description('Azure region for monitoring resources.')
param location string

@description('Log Analytics workspace name.')
param workspaceName string

@description('Application Insights component name.')
param appInsightsName string

@description('Tags applied to resources.')
param tags object = {}

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: workspaceName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  tags: tags
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

@description('Log Analytics workspace resource ID.')
output workspaceId string = workspace.id

@description('Application Insights component resource ID.')
output applicationInsightsId string = appInsights.id

@description('Application Insights connection string.')
output applicationInsightsConnectionString string = appInsights.properties.ConnectionString

@description('Application Insights instrumentation key. Prefer connection string for new apps.')
output instrumentationKey string = appInsights.properties.InstrumentationKey
