# Bicep deployment tutorial for Azure Functions Fundamentals

This tutorial explains how the course infrastructure in `deploy/bicep/` is built and how to deploy it from an empty Azure resource group. The cloud deployment mirrors the local topology from `infra-local`: Storage, Service Bus queues `orders`, `orders-out`, `enrich-in`, topic `order-events` with `audit` and `fulfilment` subscriptions, SQL database `LearningDb`, and Cosmos DB SQL database `LearningDb` with containers `orders`, `orders-leases`, `audit`, and `products`.

## What Bicep is

Bicep is Azure's domain-specific language for Azure Resource Manager (ARM) deployments. It compiles to ARM JSON, but is much easier to read and maintain:

- **Bicep vs ARM JSON**: same ARM engine and Azure support, less syntax noise, symbolic resource names, modules, type checking, and better tooling.
- **Bicep vs Terraform**: Bicep is Azure-native and has day-one support for Azure resource types through ARM. Terraform is multi-cloud and keeps its own state file. Use Terraform when you want one tool across clouds; use Bicep when you want a simple Azure-native path with no separate state backend.

Install or update Bicep through Azure CLI:

```bash
az bicep install
az bicep upgrade
az bicep version
```

You can also install the standalone `bicep` CLI, but the course commands use `az bicep`.

## Anatomy of a Bicep file

A Bicep file usually contains:

```bicep
targetScope = 'resourceGroup'

@description('Where to deploy resources.')
param location string = resourceGroup().location

var normalizedName = toLower('example')

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: '${normalizedName}${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

output storageName string = storage.name
```

Key concepts:

- `targetScope` selects deployment scope: `resourceGroup`, `subscription`, `managementGroup`, or `tenant`. This solution uses `resourceGroup`.
- `param` declares inputs. Decorators such as `@description`, `@secure`, `@minLength`, and `@maxLength` document and validate them.
- `var` declares computed values.
- `resource` declares Azure resources using `Provider/type@apiVersion`.
- `module` calls another Bicep file and passes parameters to it.
- `output` returns values after deployment.
- Loops use `[for item in items: { ... }]` to create repeated resources such as queues or containers.
- Conditions use `if (condition)` on resources or modules when deployment should be optional.

## Solution template walkthrough

### `main.bicep`

`main.bicep` is the orchestrator. It accepts:

- `location`
- `environmentName`
- `resourceNamePrefix`
- `sqlAdminLogin`
- secure `sqlAdminPassword`
- `cosmosAutoscaleMaxThroughput`
- `tags`

It computes stable names with `uniqueString`, then calls each module. Its outputs include the Function App name, Function managed identity principal ID, Application Insights connection string, Service Bus namespace, Cosmos endpoint, and SQL server FQDN.

### `modules/storage.bicep`

Creates one StorageV2 account for Azure Functions host storage and course storage examples. It also creates:

- Blob containers `uploads`, `processed`, and `function-releases`
- Storage queue `incoming-jobs`

`function-releases` is used by the Flex Consumption deployment package setting. `AzureWebJobsStorage` is currently emitted as a secure connection string because the Functions host storage connection still needs a full host storage setting for broad compatibility with course modules.

### `modules/loganalytics-appinsights.bicep`

Creates:

- Log Analytics workspace
- Workspace-based Application Insights component

Workspace-based Application Insights is the current recommended model. The Function App receives `APPLICATIONINSIGHTS_CONNECTION_STRING` so the .NET isolated worker can emit telemetry.

### `modules/servicebus.bicep`

Creates a Standard Service Bus namespace with the same entities used locally:

- Queues: `orders`, `orders-out`, `enrich-in`
- Topic: `order-events`
- Subscriptions: `audit`, `fulfilment`

The Function App uses identity-based Service Bus configuration with:

```text
ServiceBusConnection__fullyQualifiedNamespace=<namespace>.servicebus.windows.net
```

### `modules/cosmosdb.bicep`

Creates a Cosmos DB account using the SQL API, database `LearningDb`, and containers:

| Container | Partition key | TTL |
|---|---|---|
| `orders` | `/customerId` | off (`-1`) |
| `orders-leases` | `/id` | off (`-1`) |
| `audit` | `/customerId` | 30 days (`2592000`) |
| `products` | `/category` | off (`-1`) |

The database uses autoscale throughput shared by the containers. The `audit` TTL is included as a concrete TTL example for event/audit data.

### `modules/sql.bicep`

Creates:

- Azure SQL logical server
- Database `LearningDb`
- Firewall rule `AllowAllWindowsAzureIps` so Azure-hosted services can connect

The template requires a SQL admin login and secure password for initial provisioning. For production, configure a Microsoft Entra administrator and database users/groups rather than relying on SQL authentication. The Function App setting uses an Azure SQL connection string with `Authentication=Active Directory Managed Identity`; after deployment, create the matching database user and grant the required permissions in `LearningDb`.

### `modules/functionapp.bicep`

Creates:

- Flex Consumption plan (`FC1`)
- Linux Function App
- .NET isolated runtime version `10.0`
- System-assigned managed identity
- Application Insights settings
- Course app settings

Important app settings:

```text
AzureWebJobsStorage=<storage connection string>
ServiceBusConnection__fullyQualifiedNamespace=<namespace>.servicebus.windows.net
CosmosDbConnection__accountEndpoint=<cosmos endpoint>
SqlConnectionString=Server=tcp:<server>,1433;Database=LearningDb;Authentication=Active Directory Managed Identity;...
```

### `modules/rbac.bicep`

Grants the Function App managed identity:

- Azure Service Bus Data Sender
- Azure Service Bus Data Receiver
- Storage Blob Data Contributor
- Storage Queue Data Contributor
- Cosmos DB built-in Data Contributor data-plane role

## Managed identity vs connection strings

A connection string usually contains a secret. If it leaks, anyone with the secret can connect until the secret is rotated. A managed identity is an Azure AD identity attached to the Function App. Azure issues tokens to that app at runtime, and access is controlled through RBAC.

This solution prefers managed identity for Service Bus, Cosmos DB, Storage data access, and SQL runtime access. Storage still outputs `AzureWebJobsStorage` as a secure connection string for host compatibility. SQL also needs a database principal created after deployment, because Azure RBAC alone does not grant permissions inside the SQL database.

Example SQL setup after configuring an Entra admin and connecting as that admin:

```sql
CREATE USER [<function-app-name>] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [<function-app-name>];
ALTER ROLE db_datawriter ADD MEMBER [<function-app-name>];
```

Grant only the permissions your module needs in real production systems.

## Deploy from scratch

Set variables:

```bash
RG=rg-aff-dev
LOCATION=westeurope
PREFIX=affdemo
ENV=dev
SQL_ADMIN=sqladminuser
```

Create a resource group:

```bash
az group create \
  --name $RG \
  --location $LOCATION
```

Preview with what-if. You can pass the secure SQL password interactively:

```bash
az deployment group what-if \
  --resource-group $RG \
  --template-file AzureFunctionsFundamentals/deploy/bicep/main.bicep \
  --parameters location=$LOCATION \
               environmentName=$ENV \
               resourceNamePrefix=$PREFIX \
               sqlAdminLogin=$SQL_ADMIN \
               sqlAdminPassword='<enter-a-strong-password>'
```

Deploy:

```bash
az deployment group create \
  --resource-group $RG \
  --template-file AzureFunctionsFundamentals/deploy/bicep/main.bicep \
  --parameters location=$LOCATION \
               environmentName=$ENV \
               resourceNamePrefix=$PREFIX \
               sqlAdminLogin=$SQL_ADMIN \
               sqlAdminPassword='<enter-a-strong-password>'
```

To avoid putting the password in shell history, read it from a prompt:

```bash
read -s SQL_PASSWORD
az deployment group create \
  --resource-group $RG \
  --template-file AzureFunctionsFundamentals/deploy/bicep/main.bicep \
  --parameters location=$LOCATION \
               environmentName=$ENV \
               resourceNamePrefix=$PREFIX \
               sqlAdminLogin=$SQL_ADMIN \
               sqlAdminPassword=$SQL_PASSWORD
```

You can also use a parameters file that omits the password, Azure Key Vault references, or your CI/CD secret store.

Read deployment outputs:

```bash
az deployment group show \
  --resource-group $RG \
  --name main \
  --query properties.outputs
```

Get just the Function App name:

```bash
FUNCTION_APP=$(az deployment group show \
  --resource-group $RG \
  --name main \
  --query properties.outputs.functionAppName.value \
  -o tsv)

echo $FUNCTION_APP
```

## Publish the Function code

GitHub Actions should handle CI/CD for the course, but you can publish manually after building a module's Function project:

```bash
cd AzureFunctionsFundamentals/modules/01-http-trigger/examples
func azure functionapp publish $FUNCTION_APP
```

For zip deployment, publish locally and deploy the package:

```bash
dotnet publish -c Release -o publish
cd publish
zip -r ../function.zip .
az functionapp deployment source config-zip \
  --resource-group $RG \
  --name $FUNCTION_APP \
  --src ../function.zip
```

Use the module you are teaching or testing; this repository contains multiple course modules, not one monolithic Function project.

## Clean up

Delete the whole resource group when finished:

```bash
az group delete --name $RG --yes --no-wait
```

This removes the Function App, Service Bus, SQL, Cosmos DB, Storage, Application Insights, and Log Analytics resources created by these templates.
