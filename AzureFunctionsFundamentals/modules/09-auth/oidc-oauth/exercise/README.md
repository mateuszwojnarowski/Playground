# Exercise - Protect an Orders API

## Scenario

You own a small Orders API implemented as Azure Functions isolated worker. Operations teams need a public health endpoint, but order data must be protected. Clients must send an access token issued for this API and the token must include `orders.read` as a scope or role.

## Endpoints

- `GET /api/health` - public, returns `{ "status": "ok" }`.
- `GET /api/orders` - protected, validates the bearer token and requires `orders.read`.

## Acceptance criteria

- [ ] Function projects target `net10.0`, Azure Functions v4, isolated worker, `OutputType` `Exe`.
- [ ] `GET /api/health` works without a token.
- [ ] `GET /api/orders` returns `401` when the bearer token is missing, expired, from the wrong issuer, for the wrong audience, or signed by an untrusted key.
- [ ] `GET /api/orders` returns `403` when the token is valid but lacks `orders.read` in `scp`, `scope`, `role`, or `roles`.
- [ ] `GET /api/orders` returns sample orders when the token is valid and contains `orders.read`.
- [ ] Authorization logic is in `TokenAuthorizer`, a testable class independent of the Functions runtime.
- [ ] Tests are deterministic and offline.

## Run locally

Start your local OIDC provider, such as the repo's IdentityServerQuickstart / Duende IdentityServer if available, and configure it with:

- API audience/resource: `orders-api`
- Scope or role: `orders.read`
- Authority URL: `http://localhost:5001` (adjust to your local provider)

Then run the Function:

```bash
dotnet run --project modules/09-auth-oidc-oauth2/exercise/AuthOidcExercise.csproj
```

Get an access token from the local provider. The exact command depends on your IdentityServer client. For a client-credentials client it usually looks like:

```bash
curl -X POST http://localhost:5001/connect/token   -H "Content-Type: application/x-www-form-urlencoded"   -d "grant_type=client_credentials"   -d "client_id=<client-id>"   -d "client_secret=<client-secret>"   -d "scope=orders.read"
```

Call the API:

```bash
curl http://localhost:7071/api/health
curl http://localhost:7071/api/orders -H "Authorization: ******"
```

`local.settings.json` includes placeholder local settings plus Entra ID examples. It uses `Auth:RequireHttpsMetadata=false` only because the local authority placeholder is HTTP.

## Point it at Microsoft Entra ID for Azure

1. Register an API app in Entra ID.
2. Expose an API scope named `orders.read` or define an application role named `orders.read`.
3. Register or choose a client app and grant it permission to request that scope/role.
4. In the Azure Function app settings, set:

```text
Auth:Authority=https://login.microsoftonline.com/<tenant-id>/v2.0
Auth:Audience=api://<application-client-id>
Auth:RequiredPermission=orders.read
Auth:RequireHttpsMetadata=true
```

No code changes are required because Entra ID also publishes OIDC discovery and JWKS signing keys.

## Tests

Run from the repository's `AzureFunctionsFundamentals` folder:

```bash
dotnet test modules/09-auth-oidc-oauth2/exercise/tests/AuthOidcExercise.Tests.csproj
```

The tests construct `ClaimsPrincipal` instances directly for scope decisions and generate RSA keys in memory for signed JWT validation. They do not call a network identity provider.
