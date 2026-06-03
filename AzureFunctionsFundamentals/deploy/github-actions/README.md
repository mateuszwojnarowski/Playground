# GitHub Actions workflow templates

This folder contains workflow templates for the Azure Functions Fundamentals course. They are intentionally stored here instead of in the repository's real `.github/workflows/` folder so you can study and adapt them before enabling automation.

When you are ready, copy the desired files into `.github/workflows/` in your own repository.

## Templates

- `ci.yml` — runs on push and pull request. It checks out the repo, installs the .NET 10 SDK, restores, builds, and tests `AzureFunctionsFundamentals/AzureFunctionsFundamentals.sln`.
- `deploy-infra.yml` — manually deploys the Bicep template at `AzureFunctionsFundamentals/deploy/bicep/main.bicep` to an existing resource group. It uses `azure/login@v2` with GitHub OIDC and a protected GitHub Environment.
- `deploy-functions.yml` — manually publishes one parameterized .NET 10 isolated Functions project and deploys the published output with `Azure/functions-action@v1`. Run it after infrastructure exists.

## Expected flow

1. Use `ci.yml` for every push and pull request.
2. Use `deploy-infra.yml` to create or update Azure resources.
3. Use `deploy-functions.yml` to publish one Functions app project to the Function App created by the infrastructure deployment.

The deploy workflows expect repository or environment variables named `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, and `AZURE_SUBSCRIPTION_ID`. They do not require stored Azure client secrets or publish profiles.
