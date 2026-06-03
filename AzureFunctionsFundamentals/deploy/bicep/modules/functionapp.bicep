@description('Azure region for the Function App.')
param location string

@description('Function App name.')
param functionAppName string

@description('Flex Consumption plan name.')
param planName string

@description('Storage account name used by the Function App.')
param storageAccountName string

@secure()
@description('Storage connection string for AzureWebJobsStorage.')
param storageConnectionString string

@description('Blob container used by Flex Consumption package deployment.')
param deploymentContainerName string

@description('Application Insights connection string.')
param appInsightsConnectionString string

@description('Service Bus fully qualified namespace, for example name.servicebus.windows.net.')
param serviceBusFullyQualifiedNamespace string

@description('Cosmos DB account endpoint.')
param cosmosAccountEndpoint string

@description('Azure SQL server fully qualified domain name.')
param sqlServerFullyQualifiedDomainName string

@description('Azure SQL database name.')
param sqlDatabaseName string

@description('Tags applied to resources.')
param tags object = {}

var deploymentContainerUrl = 'https://${storageAccountName}.blob.${environment().suffixes.storage}/${deploymentContainerName}'
var sqlManagedIdentityConnectionString = 'Server=tcp:${sqlServerFullyQualifiedDomainName},1433;Database=${sqlDatabaseName};Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: planName
  location: location
  tags: tags
  kind: 'functionapp'
  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }
  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  tags: tags
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: deploymentContainerUrl
          authentication: {
            type: 'SystemAssignedIdentity'
          }
        }
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '10.0'
      }
      scaleAndConcurrency: {
        maximumInstanceCount: 40
        instanceMemoryMB: 2048
      }
    }
    siteConfig: {
      minTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageConnectionString
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'ServiceBusConnection__fullyQualifiedNamespace'
          value: serviceBusFullyQualifiedNamespace
        }
        {
          name: 'CosmosDbConnection__accountEndpoint'
          value: cosmosAccountEndpoint
        }
        {
          name: 'SqlConnectionString'
          value: sqlManagedIdentityConnectionString
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
      ]
    }
  }
}

@description('Function App name.')
output functionAppName string = functionApp.name

@description('System-assigned managed identity principal ID.')
output principalId string = functionApp.identity.principalId

@description('Default Function App host name.')
output defaultHostName string = functionApp.properties.defaultHostName
