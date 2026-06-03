@description('Azure region for Cosmos DB.')
param location string

@description('Globally unique Cosmos DB account name.')
param accountName string

@description('Cosmos DB SQL database name.')
param databaseName string = 'LearningDb'

@description('Autoscale maximum RU/s for the database. Containers share database throughput.')
@minValue(1000)
param autoscaleMaxThroughput int = 1000

@description('Tags applied to resources.')
param tags object = {}

var containers = [
  {
    name: 'orders'
    partitionKey: '/customerId'
    ttl: -1
  }
  {
    name: 'orders-leases'
    partitionKey: '/id'
    ttl: -1
  }
  {
    name: 'audit'
    partitionKey: '/customerId'
    ttl: 2592000
  }
  {
    name: 'products'
    partitionKey: '/category'
    ttl: -1
  }
]

resource account 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: accountName
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    enableAutomaticFailover: false
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
    capabilities: []
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: account
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
    options: {
      autoscaleSettings: {
        maxThroughput: autoscaleMaxThroughput
      }
    }
  }
}

resource sqlContainers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = [for container in containers: {
  parent: database
  name: container.name
  properties: {
    resource: {
      id: container.name
      partitionKey: {
        paths: [
          container.partitionKey
        ]
        kind: 'Hash'
      }
      defaultTtl: container.ttl
    }
    options: {}
  }
}]

@description('Cosmos DB account resource name.')
output accountName string = account.name

@description('Cosmos DB account endpoint.')
output accountEndpoint string = account.properties.documentEndpoint

@description('Cosmos DB SQL database name.')
output databaseName string = database.name

@description('Cosmos DB account resource ID.')
output accountId string = account.id
