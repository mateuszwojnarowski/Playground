@description('Azure region for Service Bus.')
param location string

@description('Globally unique Service Bus namespace name.')
param namespaceName string

@description('Tags applied to resources.')
param tags object = {}

var queueNames = [
  'orders'
  'orders-out'
  'enrich-in'
]
var subscriptionNames = [
  'audit'
  'fulfilment'
]

resource namespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: namespaceName
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}

resource queues 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = [for queueName in queueNames: {
  parent: namespace
  name: queueName
  properties: {
    lockDuration: 'PT1M'
    maxDeliveryCount: 10
    requiresDuplicateDetection: false
    requiresSession: false
    deadLetteringOnMessageExpiration: true
  }
}]

resource orderEventsTopic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' = {
  parent: namespace
  name: 'order-events'
  properties: {
    defaultMessageTimeToLive: 'P14D'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    supportOrdering: true
  }
}

resource subscriptions 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2024-01-01' = [for subscriptionName in subscriptionNames: {
  parent: orderEventsTopic
  name: subscriptionName
  properties: {
    lockDuration: 'PT1M'
    maxDeliveryCount: 10
    deadLetteringOnMessageExpiration: true
  }
}]

@description('Service Bus namespace resource name.')
output namespaceName string = namespace.name

@description('Service Bus fully qualified namespace for identity-based connections.')
output fullyQualifiedNamespace string = '${namespace.name}.servicebus.windows.net'

@description('Service Bus namespace resource ID.')
output namespaceId string = namespace.id
