# Deploy Azure Functions with GitHub Actions and OIDC

Yes, you can deploy Azure Functions with GitHub Actions. A typical production-friendly flow is:

1. **CI**: build and test the .NET solution on every push and pull request.
2. **Infrastructure deployment**: deploy Azure resources from Bicep, usually behind a manual approval gate.
3. **Application deployment**: publish one .NET isolated Functions project and deploy the publish output to an existing Function App.

The workflow templates for this course live in `deploy/github-actions/`. They are examples to copy into your repository's real `.github/workflows/` folder when you are ready to enable them.

## Why use OIDC instead of stored secrets?

Older deployment examples often use a Function App publish profile or an Azure service principal client secret saved as a GitHub secret. Those work, but they create long-lived credentials that can be copied, leaked, or forgotten.

OpenID Connect (OIDC) federation is safer because GitHub asks Azure for a short-lived token at workflow runtime. Azure only issues that token when the workflow matches a federated credential rule you configured, such as a specific repository, branch, pull request, or GitHub Environment.

Benefits:

- No publish profile in GitHub.
- No service principal password in GitHub.
- Tokens are short-lived.
- Access can be limited with Azure RBAC at a resource group scope.
- GitHub Environment approvals can be part of the trust boundary.

## Identity setup overview

You can federate GitHub Actions with either an Entra ID app registration or a user-assigned managed identity. Both support OIDC federated credentials. For a course or small application, an app registration is simple. In larger Azure estates, a user-assigned managed identity can fit better with existing governance.

You need four things:

1. An Azure identity that GitHub Actions can federate into.
2. A federated credential on that identity that matches your GitHub workflow context.
3. Azure RBAC permissions on the target resource group.
4. GitHub variables containing the Azure tenant, subscription, and client ID.

## Option A: create an Entra app registration

Replace placeholders before running commands.

```bash
APP_NAME="azfunc-fundamentals-github-actions"
az ad app create --display-name "$APP_NAME"
APP_ID=$(az ad app list --display-name "$APP_NAME" --query "[0].appId" -o tsv)
az ad sp create --id "$APP_ID"
echo "$APP_ID"
```

`APP_ID` is the value to store as `AZURE_CLIENT_ID` in GitHub.

## Option B: create a user-assigned managed identity

```bash
IDENTITY_RG="rg-github-identities"
LOCATION="westeurope"
IDENTITY_NAME="id-azfunc-fundamentals-github-actions"

az group create --name "$IDENTITY_RG" --location "$LOCATION"
az identity create \
  --resource-group "$IDENTITY_RG" \
  --name "$IDENTITY_NAME" \
  --location "$LOCATION"

CLIENT_ID=$(az identity show \
  --resource-group "$IDENTITY_RG" \
  --name "$IDENTITY_NAME" \
  --query clientId -o tsv)
echo "$CLIENT_ID"
```

`CLIENT_ID` is the value to store as `AZURE_CLIENT_ID` in GitHub.

## Add federated credentials

A federated credential tells Azure which GitHub workflow identities are trusted. The important fields are:

- **Issuer**: `https://token.actions.githubusercontent.com`
- **Audience**: `api://AzureADTokenExchange`
- **Subject**: the GitHub identity pattern Azure should trust

Common subject formats:

- Branch: `repo:OWNER/REPO:ref:refs/heads/main`
- Pull request: `repo:OWNER/REPO:pull_request`
- GitHub Environment: `repo:OWNER/REPO:environment:dev`

For deployment workflows, prefer an Environment subject because it pairs well with manual approvals.

### App registration federated credential

Create a JSON file such as `credential-dev.json` locally:

```json
{
  "name": "github-dev-environment",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:OWNER/REPO:environment:dev",
  "description": "GitHub Actions deployments from the dev environment",
  "audiences": ["api://AzureADTokenExchange"]
}
```

Then run:

```bash
az ad app federated-credential create \
  --id "$APP_ID" \
  --parameters credential-dev.json
```

### User-assigned managed identity federated credential

```bash
az identity federated-credential create \
  --resource-group "$IDENTITY_RG" \
  --identity-name "$IDENTITY_NAME" \
  --name "github-dev-environment" \
  --issuer "https://token.actions.githubusercontent.com" \
  --subject "repo:OWNER/REPO:environment:dev" \
  --audience "api://AzureADTokenExchange"
```

Create separate federated credentials for each trusted environment, branch, or repo pattern. Do not make the subject broader than needed.

## Assign Azure RBAC

The identity needs permission on the target resource group.

For infrastructure deployment, `Contributor` on the resource group is usually enough for this course:

```bash
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
TARGET_RG="rg-functions-fundamentals-dev"

az role assignment create \
  --assignee "$APP_ID" \
  --role Contributor \
  --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$TARGET_RG"
```

If you used a managed identity, pass its client ID as `--assignee`. In production, use least privilege and consider separate identities for infrastructure and application deployment.

RBAC can take a few minutes to propagate. If the first workflow fails with authorization errors, wait and rerun it.

## Configure GitHub variables

In GitHub, go to **Settings → Secrets and variables → Actions → Variables** and add:

- `AZURE_CLIENT_ID`: the app registration app ID or managed identity client ID.
- `AZURE_TENANT_ID`: your Entra tenant ID.
- `AZURE_SUBSCRIPTION_ID`: the Azure subscription ID.

These are identifiers, not passwords, so variables are appropriate. Use secrets for actual sensitive values. The workflow templates do not require Azure client secrets or Function App publish profiles.

You can store these variables at repository level or at Environment level. Environment-level variables are useful when `dev`, `test`, and `prod` use different Azure identities or subscriptions.

## Required workflow permissions

OIDC requires this permissions block:

```yaml
permissions:
  id-token: write
  contents: read
```

`id-token: write` lets GitHub mint an OIDC token for the job. `contents: read` lets `actions/checkout` read the repository.

`azure/login@v2` exchanges the GitHub OIDC token for an Azure access token:

```yaml
- name: Azure login with OIDC
  uses: azure/login@v2
  with:
    client-id: ${{ vars.AZURE_CLIENT_ID }}
    tenant-id: ${{ vars.AZURE_TENANT_ID }}
    subscription-id: ${{ vars.AZURE_SUBSCRIPTION_ID }}
```

No `client-secret` is supplied.

## GitHub Environments and manual approvals

Deployment workflows in this course use:

```yaml
environment: ${{ inputs.environment_name }}
```

Create matching GitHub Environments such as `dev`, `test`, and `prod` in **Settings → Environments**. For sensitive environments, add protection rules:

- Required reviewers for manual approval.
- Deployment branch restrictions.
- Environment-specific variables.
- Environment-specific secrets if your app needs non-Azure sensitive values.

When a job targets a protected environment, GitHub pauses the job until approval is granted. If your federated credential subject is `repo:OWNER/REPO:environment:prod`, Azure also requires the job to run in that exact Environment before issuing a token.

## Workflow walkthrough

### `deploy/github-actions/ci.yml`

This workflow runs on push and pull request for changes under `AzureFunctionsFundamentals/**`. It:

1. Checks out the repository with `actions/checkout@v4`.
2. Installs the .NET 10 SDK with `actions/setup-dotnet@v4` and `dotnet-version: 10.0.x`.
3. Restores `AzureFunctionsFundamentals/AzureFunctionsFundamentals.sln`.
4. Builds the solution in Release configuration.
5. Runs the xUnit tests with `dotnet test`.

This workflow does not need OIDC because it only reads source code and runs local build/test commands.

### `deploy/github-actions/deploy-infra.yml`

This workflow is manually triggered with `workflow_dispatch`. It asks for:

- `environment_name`: the GitHub Environment to use.
- `resource_group_name`: the existing resource group to deploy into.
- `resource_prefix`: the name prefix passed to Bicep.
- `location`: the Azure region passed to Bicep.

It logs in with `azure/login@v2` using OIDC, then deploys `AzureFunctionsFundamentals/deploy/bicep/main.bicep` with `azure/arm-deploy@v2` at resource group scope.

The workflow includes a commented Azure CLI alternative using `az deployment group create` if you prefer direct CLI commands.

### `deploy/github-actions/deploy-functions.yml`

This workflow is also manually triggered. It asks for:

- `environment_name`: the GitHub Environment to use.
- `function_app_name`: the Azure Function App name created by infrastructure.
- `project_path`: the path to one `.csproj` file for a .NET 10 isolated Functions app.
- `dotnet_configuration`: usually `Release`.

It installs .NET 10, logs in to Azure with OIDC, restores the selected project, publishes it, and deploys the publish folder:

```yaml
- name: Publish selected .NET 10 isolated Functions project
  run: >-
    dotnet publish "${{ inputs.project_path }}"
    --configuration "${{ inputs.dotnet_configuration }}"
    --output "${{ env.PUBLISH_OUTPUT }}"
    --no-restore

- name: Deploy package to Azure Functions
  uses: Azure/functions-action@v1
  with:
    app-name: ${{ inputs.function_app_name }}
    package: ${{ env.PUBLISH_OUTPUT }}
```

For isolated worker Functions, `dotnet publish` creates the app files that Azure Functions needs, including the executable, dependencies, `host.json`, and generated function metadata. The Functions action uploads that folder to the named Function App.

Run infrastructure deployment first so the Function App exists before application deployment.

## Copying templates into real workflows

These files are not active while they remain in `deploy/github-actions/`. To enable one:

1. Create `.github/workflows/` in your repository.
2. Copy the desired YAML file into that folder.
3. Adjust paths if your repository layout differs.
4. Commit the workflow.
5. Configure GitHub variables and Environments.
6. Run CI or manually dispatch the deployment workflow.

Do not commit publish profiles or Azure client secrets.

## Runtime notes for .NET 10 isolated Functions

The course projects target:

- .NET 10 / `net10.0`
- Azure Functions v4
- Isolated worker model / `dotnet-isolated`

Make sure the Azure Function App is configured for a compatible Functions runtime and .NET isolated worker runtime. If the Bicep template exposes runtime parameters, keep them aligned with the project target framework. A mismatch can build successfully but fail at startup in Azure.

## Troubleshooting

### `azure/login` fails with an audience error

Check that the federated credential audience is exactly:

```text
api://AzureADTokenExchange
```

If you customized the audience in `azure/login`, the Azure federated credential must match it. The templates use the default audience.

### `azure/login` says no matching federated identity record was found

The subject probably does not match the workflow context. Compare the federated credential subject with the workflow run:

- Environment deployment needs `repo:OWNER/REPO:environment:ENVIRONMENT_NAME`.
- Main branch deployment needs `repo:OWNER/REPO:ref:refs/heads/main`.
- Pull request workflows need `repo:OWNER/REPO:pull_request`.

Subjects are case-sensitive for owner, repo, and environment names. If the workflow uses `environment: prod`, a credential for `environment:production` will not match.

### Deployment fails with authorization or `AuthorizationFailed`

The identity logged in successfully but lacks RBAC on the target scope, or RBAC has not propagated yet. Confirm the role assignment:

```bash
az role assignment list \
  --assignee "AZURE_CLIENT_ID" \
  --scope "/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME" \
  -o table
```

Wait a few minutes after creating RBAC assignments, then rerun the workflow.

### Bicep deployment cannot find `main.bicep`

The template path in the workflow is:

```text
AzureFunctionsFundamentals/deploy/bicep/main.bicep
```

If you copy the course content into a different layout, update `BICEP_FILE` in `deploy-infra.yml`.

### Function App deployment cannot find the project

Check the `project_path` workflow input. It must point to one Functions `.csproj`, not a directory and not the solution file.

### App deploy succeeds but functions do not appear

Common causes:

- The project is not an Azure Functions isolated worker project.
- `host.json` was not copied to publish output.
- Function methods are missing trigger attributes.
- The Function App runtime stack does not match .NET isolated.
- The wrong Function App name was supplied.

Download or inspect the deployment package output in the workflow logs and verify it contains `host.json` and generated function metadata.

### Runtime or runtime-version mismatch

The project targets `net10.0`, so the Function App must support the .NET isolated runtime version you deploy. If Azure reports startup errors, check:

- Function App stack: .NET isolated, not in-process .NET.
- `FUNCTIONS_WORKER_RUNTIME`: `dotnet-isolated`.
- `FUNCTIONS_EXTENSION_VERSION`: `~4`.
- Any Bicep runtime settings in `deploy/bicep/main.bicep`.

### `Azure/functions-action` asks for a publish profile

Do not add a publish profile for these templates. Make sure `azure/login@v2` ran earlier in the same job and succeeded. The Functions action can use the Azure identity from the login step.

### OIDC worked yesterday but fails today

Check whether the workflow branch, repository name, or Environment name changed. Federated credential subjects must match exactly. Also confirm the Azure identity still exists and still has RBAC on the target resource group.
