# Exercise - Protect an Orders API

## Scenario

You own a small Orders API implemented as Azure Functions isolated worker.
Operations teams need a public health endpoint, but order data must be protected.
Clients must send an access token issued for this API by your OIDC/OAuth provider,
and the token must include `orders.read` as a scope or role.

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

Stand up an OIDC/OAuth provider backed by **ASP.NET Core Identity** (for example an
ASP.NET Core host using the open-source OpenIddict server on top of Identity), and
configure it with:

- API audience/resource: `orders-api`
- Scope or role: `orders.read`
- Authority URL: `https://localhost:5001` (adjust to your provider)

Then run the Function:

```bash
dotnet run --project modules/09-auth/oidc-oauth/exercise/OidcOAuthExercise.csproj
```

Obtain an access token from the provider. For a client-credentials client the
request usually looks like:

```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=<client-id>" \
  -d "client_secret=<client-secret>" \
  -d "scope=orders.read"
```

Call the API (curl sets the `Authorization` header from the token):

```bash
curl http://localhost:7071/api/health
curl --oauth2-bearer "$ACCESS_TOKEN" http://localhost:7071/api/orders
```

`local.settings.json` includes placeholder local settings. Set
`Auth:RequireHttpsMetadata=true` whenever the authority is served over HTTPS.

## Switching providers

Because every OIDC-compliant provider publishes the same discovery and JWKS
metadata, you move between an ASP.NET Core Identity/OpenIddict host, a managed cloud
identity provider, or any other compliant server by changing configuration only:

```text
Auth:Authority=https://<your-authority>
Auth:Audience=orders-api
Auth:RequiredPermission=orders.read
Auth:RequireHttpsMetadata=true
```

No code changes are required.

## Tests

Run from the repository's `AzureFunctionsFundamentals` folder:

```bash
dotnet test modules/09-auth/oidc-oauth/exercise/tests/OidcOAuthExercise.Tests.csproj
```

The tests construct `ClaimsPrincipal` instances directly for scope decisions and
generate RSA keys in memory for signed JWT validation. They do not call a network
identity provider.
