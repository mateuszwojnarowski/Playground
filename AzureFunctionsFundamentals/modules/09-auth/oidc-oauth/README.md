# Module 09 - Token-based auth with OIDC and OAuth 2.0

This module shows how to protect Azure Functions (.NET 10 isolated worker) with bearer tokens issued by an OpenID Connect (OIDC) provider. The examples use the same validation pattern you use in Azure: read the `Authorization` header, discover signing keys from the identity provider, validate the JWT, then make an authorization decision from claims such as scopes or roles.

## OAuth 2.0 and OIDC in plain language

OAuth 2.0 is an authorization framework: it lets an app call an API on behalf of a user or service without sharing passwords with the API. The common roles are:

- **Resource owner**: the user or workload that owns the data.
- **Client**: the app requesting access, such as a SPA, mobile app, CLI, daemon, or backend service.
- **Authorization server**: the identity provider that authenticates the owner/client and issues tokens. Examples: Microsoft Entra ID, Duende IdentityServer, Auth0, Okta.
- **Resource server**: the API receiving the token. In this module, the Azure Function is the resource server.

OIDC builds on OAuth 2.0 by adding authentication. OAuth 2.0 answers "may this client call this API?" OIDC also answers "who signed in?" OIDC standardizes identity claims and the discovery document at `/.well-known/openid-configuration`.

### Access tokens vs ID tokens

- **Access token**: sent to an API in `Authorization: ****** The Function validates it and checks scopes/roles. APIs should use access tokens.
- **ID token**: sent to a client application after sign-in to describe the signed-in user. It proves authentication to the client, not authorization to your API. Do not use ID tokens as API bearer tokens.

### Scopes, roles, and claims

A **claim** is a named fact in a token, such as `sub` (subject/user id), `name`, `iss` (issuer), `aud` (audience), `exp` (expiry), `scp`/`scope` (delegated permissions), or `role`/`roles` (application roles). A **scope** such as `orders.read` means the token permits a limited operation. A **role** is often used for app-only or coarse-grained permissions.

### JWT structure and signing keys

Most access tokens are JWTs: `header.payload.signature`.

- The **header** identifies the algorithm and key id (`kid`).
- The **payload** contains claims.
- The **signature** proves the token was issued by the identity provider and has not been changed.

The Function does not store the issuer's private key. It downloads public signing keys from the provider's JWKS endpoint, usually discovered from the OIDC metadata document. Key rotation is why production code should read keys from discovery/JWKS rather than copy a single key into configuration.

### Validation checks an API must perform

A protected Function must validate all of these:

1. **Signature/signing key**: the JWT signature matches a trusted key from the issuer's JWKS.
2. **Issuer**: `iss` is the identity provider/tenant you trust.
3. **Audience**: `aud` is your API identifier, not another API.
4. **Lifetime**: `exp`/`nbf` show the token is currently valid.
5. **Authorization claims**: required `scope`/`scp` or `role`/`roles` is present for the operation.

Never disable issuer, audience, lifetime, or signing key validation for convenience.

## Common flows

### Authorization Code + PKCE

Use this for interactive users in SPAs, native apps, and browser-based apps. The user signs in at the authorization server. The client receives an authorization code, proves it is the same client with PKCE, and exchanges the code for tokens. The API receives only the access token.

Use when: a human signs in and the app calls Functions on that user's behalf.

### Client Credentials

Use this for service-to-service calls. A daemon, workflow, or backend service authenticates as itself and receives an app-only access token. There is no human user.

Use when: a job, integration service, or another API calls your Function without user interaction.

## How a Function validates a bearer token

This module's projects use a `JwtBearerValidator` behind an `ITokenValidator` abstraction:

1. Read `Authorization` from the HTTP request.
2. Require the `****** scheme.
3. Load OIDC discovery from `{Authority}/.well-known/openid-configuration`.
4. Read the issuer and JWKS signing keys from discovery.
5. Validate issuer, audience, lifetime, and signing key with `JwtSecurityTokenHandler`.
6. Return a `ClaimsPrincipal` to the Function.
7. The Function or a testable service checks required scopes/roles.

## Local identity provider approach

For local learning, run a local OIDC provider. This repo may include an IdentityServerQuickstart / Duende IdentityServer host in your environment; point `Auth:Authority` to that server's discovery endpoint host, for example:

```json
"Auth:Authority": "http://localhost:5001",
"Auth:Audience": "orders-api",
"Auth:RequireHttpsMetadata": "false"
```

`RequireHttpsMetadata=false` is for local HTTP-only development. Use HTTPS in shared or production environments. The API still validates token signature, issuer, audience, and lifetime.

## Cloud approach with Microsoft Entra ID

The exact same validation works in Azure with Microsoft Entra ID. Change configuration, not code:

```json
"Auth:Authority": "https://login.microsoftonline.com/<tenant-id>/v2.0",
"Auth:Audience": "api://<application-client-id>",
"Auth:RequireHttpsMetadata": "true"
```

Register an API app in Entra ID, expose scopes such as `orders.read`, grant a client app permission to request them, then deploy the Function with matching app settings. Entra publishes OIDC discovery and JWKS, so the validator can trust rotated Microsoft signing keys automatically.

## Easy Auth alternative

Azure Functions on App Service also supports built-in App Service Authentication/Authorization, often called **Easy Auth**. Easy Auth can reject unauthenticated requests before your code runs and can pass user claims in headers. It is convenient for simple apps and reduces code. Code-level validation, as shown here, is more portable, easier to test offline, and gives fine-grained control over scopes, roles, and multi-provider scenarios. Some production systems combine both: Easy Auth at the edge and in-code authorization for business permissions.

## How to set this up

1. Create or choose an OIDC provider: local Duende IdentityServer for development or Microsoft Entra ID for Azure.
2. Register an API/resource with an audience such as `orders-api`.
3. Define scopes/roles, for example `orders.read`.
4. Configure the Function app settings:
   - `Auth:Authority`: issuer base URL, not the JWKS URL.
   - `Auth:Audience`: expected API audience.
   - `Auth:RequiredPermission`: protected operation permission for the exercise.
   - `Auth:RequireHttpsMetadata`: `false` only for local HTTP IdentityServer; `true` in Azure.
5. Obtain an access token for the API.
6. Call protected endpoints with `Authorization: ******
7. Keep token validation in injectable classes so tests do not need the Functions runtime or a live IdP.

## Projects

- `examples/`: minimal protected HTTP Function returning caller claims.
- `exercise/`: small orders API with public health and protected orders endpoints.
- `exercise/tests/`: deterministic xUnit tests for claims authorization and offline JWT validation with generated RSA keys.
