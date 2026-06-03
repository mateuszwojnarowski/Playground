targetScope = 'resourceGroup'

@description('Azure region for all resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('Short environment name, for example dev, test, or prod.')
@minLength(2)
@maxLength(12)
param environmentName string = 'dev'

@description('Lowercase resource name prefix. Use 3-12 alphanumeric characters so globally unique names can be generated.')
@minLength(3)
@maxLength(12)
param resourceNamePrefix string

@description('Azure SQL administrator login name. Prefer creating an Entra ID administrator after deployment for production.')
param sqlAdminLogin string

@secure()
@description('Azure SQL administrator password. Do not commit this value; pass it at deployment time or from a secret store.')
param sqlAdminPassword string

@description('Cosmos DB autoscale maximum RU/s for the database. Containers share this throughput.')
@minValue(1000)
param cosmosAutoscaleMaxThroughput int = 1000

@description('Optional tags applied to all supported resources.')
param tags object = {
  course: 'Azure Functions Fundamentals'
  environment: environmentName
}

var normalizedPrefix = toLower(replace(resourceNamePrefix, '-', ''))
var suffix = uniqueString(resourceGroup().id, normalizedPrefix, environmentName)
var baseName = '${normalizedPrefix}-${environmentName}'
var compactBaseName = '${normalizedPrefix}${environmentName}${suffix}'

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    location: location
    storageAccountName: take(compactBaseName, 24)
    tags: tags
  }
}

module monitoring 'modules/loganalytics-appinsights.bicep' = {
  name: 'monitoring'
  params: {
    location: location
    workspaceName: take('${baseName}-law-${suffix}', 63)
    appInsightsName: take('${baseName}-appi-${suffix}', 255)
    tags: tags
  }
}

module serviceBus 'modules/servicebus.bicep' = {
  name: 'servicebus'
  params: {
    location: location
    namespaceName: take('${baseName}-sb-${suffix}', 50)
    tags: tags
  }
}

module cosmosDb 'modules/cosmosdb.bicep' = {
  name: 'cosmosdb'
  params: {
    location: location
    accountName: take('${baseName}-cosmos-${suffix}', 44)
    databaseName: 'LearningDb'
    autoscaleMaxThroughput: cosmosAutoscaleMaxThroughput
    tags: tags
  }
}

module sql 'modules/sql.bicep' = {
  name: 'sql'
  params: {
    location: location
    serverName: take('${baseName}-sql-${suffix}', 63)
    databaseName: 'LearningDb'
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    tags: tags
  }
}

module functionApp 'modules/functionapp.bicep' = {
  name: 'functionapp'
  params: {
    location: location
    functionAppName: take('${baseName}-func-${suffix}', 60)
    planName: take('${baseName}-fc-${suffix}', 40)
    storageAccountName: storage.outputs.storageAccountName
    storageConnectionString: storage.outputs.storageConnectionString
    deploymentContainerName: storage.outputs.functionDeploymentContainerName
    appInsightsConnectionString: monitoring.outputs.applicationInsightsConnectionString
    serviceBusFullyQualifiedNamespace: serviceBus.outputs.fullyQualifiedNamespace
    cosmosAccountEndpoint: cosmosDb.outputs.accountEndpoint
    sqlServerFullyQualifiedDomainName: sql.outputs.fullyQualifiedDomainName
    sqlDatabaseName: sql.outputs.databaseName
    tags: tags
  }
}

module rbac 'modules/rbac.bicep' = {
  name: 'rbac'
  params: {
    functionPrincipalId: functionApp.outputs.principalId
    serviceBusNamespaceName: serviceBus.outputs.namespaceName
    cosmosAccountName: cosmosDb.outputs.accountName
    storageAccountName: storage.outputs.storageAccountName
  }
}

@description('Name of the deployed Function App.')
output functionAppName string = functionApp.outputs.functionAppName

@description('System-assigned managed identity principal ID for the Function App.')
output functionPrincipalId string = functionApp.outputs.principalId

@description('Application Insights connection string for worker telemetry.')
output applicationInsightsConnectionString string = monitoring.outputs.applicationInsightsConnectionString

@description('Service Bus namespace host name for identity-based Functions bindings.')
output serviceBusFullyQualifiedNamespace string = serviceBus.outputs.fullyQualifiedNamespace

@description('Cosmos DB account endpoint for identity-based Functions bindings.')
output cosmosAccountEndpoint string = cosmosDb.outputs.accountEndpoint

@description('Azure SQL server fully qualified domain name.')
output sqlServerFullyQualifiedDomainName string = sql.outputs.fullyQualifiedDomainName
