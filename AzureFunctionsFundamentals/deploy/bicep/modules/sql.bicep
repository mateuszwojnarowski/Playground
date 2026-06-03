@description('Azure region for Azure SQL.')
param location string

@description('Globally unique Azure SQL logical server name.')
param serverName string

@description('Azure SQL database name.')
param databaseName string = 'LearningDb'

@description('SQL administrator login. For production, prefer configuring a Microsoft Entra administrator and limiting SQL authentication.')
param administratorLogin string

@secure()
@description('SQL administrator password. Pass securely at deployment time.')
param administratorLoginPassword string

@description('Tags applied to resources.')
param tags object = {}

resource server 'Microsoft.Sql/servers@2023-08-01' = {
  name: serverName
  location: location
  tags: tags
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource database 'Microsoft.Sql/servers/databases@2023-08-01' = {
  parent: server
  name: databaseName
  location: location
  tags: tags
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
  }
}

resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
  parent: server
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

@description('Azure SQL logical server name.')
output serverName string = server.name

@description('Azure SQL fully qualified domain name.')
output fullyQualifiedDomainName string = server.properties.fullyQualifiedDomainName

@description('Azure SQL database name.')
output databaseName string = database.name
