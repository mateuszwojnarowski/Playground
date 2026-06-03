@description('Azure region for the storage account.')
param location string

@description('Globally unique storage account name. Use 3-24 lowercase letters and numbers.')
@minLength(3)
@maxLength(24)
param storageAccountName string

@description('Tags applied to resources.')
param tags object = {}

var blobContainers = [
  'uploads'
  'processed'
  'function-releases'
]
var queues = [
  'incoming-jobs'
]

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    accessTier: 'Hot'
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = [for containerName in blobContainers: {
  parent: blobService
  name: containerName
  properties: {
    publicAccess: 'None'
  }
}]

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

resource storageQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-05-01' = [for queueName in queues: {
  parent: queueService
  name: queueName
}]

@description('Storage account resource name.')
output storageAccountName string = storageAccount.name

@description('Name of the blob container used by Flex Consumption package deployment.')
output functionDeploymentContainerName string = blobContainer[2].name

@secure()
@description('Storage connection string for AzureWebJobsStorage.')
output storageConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
