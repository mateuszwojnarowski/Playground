# OIDC / OAuth 2.0 with ASP.NET Core Identity

This module protects Azure Functions (.NET 10 isolated worker) with bearer tokens
issued by a **separate identity provider** and validated by the API. Unlike the
[`jwt-auth`](../jwt-auth/README.md) module — where one API both issues and validates
its own HS256 tokens — here an external **authorization server** issues tokens
signed with an **asymmetric** key (RS256). The API never sees the signing secret;
it downloads the provider's **public** keys and validates issuer, audience,
signature, lifetime, and scopes/roles.

For the identity provider we use **ASP.NET Core Identity** (the built-in .NET user
and credential store). To expose standards OIDC/OAuth endpoints on top of Identity,
pair it with an OpenID Connect server such as the open-source **OpenIddict**
library. The point of this module is the **validation pattern on the API side**,
which is identical no matter which compliant provider issues the tokens.

## OAuth 2.0 and OIDC in plain language

OAuth 2.0 is an authorization framework: it lets an app call an API on behalf of a
user or service without sharing passwords with the API. The common roles are:

- **Resource owner**: the user or workload that owns the data.
- **Client**: the app requesting access, such as a SPA, mobile app, CLI, daemon, or backend service.
- **Authorization server**: the identity provider that authenticates the owner/client and issues tokens. Here that is ASP.NET Core Identity fronted by an OIDC server (for example OpenIddict).
- **Resource server**: the API receiving the token. In this module, the Azure Function is the resource server.

OIDC builds on OAuth 2.0 by adding authentication. OAuth 2.0 answers "may this client
call this API?" OIDC also answers "who signed in?" OIDC standardizes identity claims
and the discovery document at `/.well-known/openid-configuration`.

### Access tokens vs ID tokens

- **Access token**: sent to an API. The Function validates it and checks scopes/roles. APIs should use access tokens.
- **ID token**: sent to a client application after sign-in to describe the signed-in user. It proves authentication to the client, not authorization to your API. Do not use ID tokens as API bearer tokens.

### Scopes, roles, and claims

A **claim** is a named fact in a token, such as `sub` (subject/user id), `name`,
`iss` (issuer), `aud` (audience), `exp` (expiry), `scp`/`scope` (delegated
permissions), or `role`/`roles` (application roles). A **scope** such as
`orders.read` means the token permits a limited operation. A **role** is often used
for app-only or coarse-grained permissions.

### JWT structure and signing keys

Most access tokens are JWTs: `header.payload.signature`.

- The **header** identifies the algorithm and key id (`kid`).
- The **payload** contains claims.
- The **signature** proves the token was issued by the identity provider and has not been changed.

The Function does not store the issuer's private key. It downloads public signing
keys from the provider's JWKS endpoint, usually discovered from the OIDC metadata
document. Key rotation is why production code should read keys from discovery/JWKS
rather than copy a single key into configuration.

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

Use this for interactive users in SPAs, native apps, and browser-based apps. The
user signs in at the authorization server. The client receives an authorization
code, proves it is the same client with PKCE, and exchanges the code for tokens.
The API receives only the access token.

Use when: a human signs in and the app calls Functions on that user's behalf.

### Client Credentials

Use this for service-to-service calls. A daemon, workflow, or backend service
authenticates as itself and receives an app-only access token. There is no human
user.

Use when: a job, integration service, or another API calls your Function without
user interaction.

## How a Function validates a bearer token

This module's projects use a `JwtBearerValidator` behind an `ITokenValidator`
abstraction:

1. Read `Authorization` from the HTTP request.
2. Require the `Bearer` scheme.
3. Load OIDC discovery from `{Authority}/.well-known/openid-configuration`.
4. Read the issuer and JWKS signing keys from discovery.
5. Validate issuer, audience, lifetime, and signing key with `JwtSecurityTokenHandler`.
6. Return a `ClaimsPrincipal` to the Function.
7. The Function or a testable service checks required scopes/roles.

## Setting up an ASP.NET Core Identity authorization server

ASP.NET Core Identity gives you user management (registration, password hashing,
lockout, MFA). On its own it is **not** a full OIDC/OAuth server, so to issue
standards tokens with discovery and JWKS you add an OIDC server library on top:

1. Create an ASP.NET Core web app and add ASP.NET Core Identity with your user store
   (`AddIdentity`/`AddIdentityCore` + a store such as Entity Framework Core).
2. Add an OpenID Connect server — for example the open-source **OpenIddict** — and
   wire it to Identity so users authenticate against the Identity store.
3. Configure RS256 signing/encryption certificates, the issuer URL, and the API as a
   **resource/audience** (for example `orders-api`).
4. Define scopes/roles such as `orders.read`.
5. The server now publishes `/.well-known/openid-configuration` and a JWKS endpoint,
   which is exactly what the Function's validator consumes.

Point the Function at that server:

```json
"Auth:Authority": "https://localhost:5001",
"Auth:Audience": "orders-api",
"Auth:RequireHttpsMetadata": "true"
```

`Auth:RequireHttpsMetadata=false` is only for local HTTP-only development. The API
still validates token signature, issuer, audience, and lifetime.

This is the free, self-hosted alternative to commercial identity products or a
managed cloud identity provider. Because every OIDC-compliant provider publishes the
same discovery and JWKS metadata, you switch providers by changing configuration —
not code.

## Easy Auth alternative

Azure Functions on App Service also supports built-in App Service
Authentication/Authorization, often called **Easy Auth**. Easy Auth can reject
unauthenticated requests before your code runs and can pass user claims in headers.
It is convenient for simple apps and reduces code. Code-level validation, as shown
here, is more portable, easier to test offline, and gives fine-grained control over
scopes, roles, and multi-provider scenarios. Some production systems combine both:
Easy Auth at the edge and in-code authorization for business permissions.

## Projects

- `examples/`: minimal protected HTTP Function returning caller claims.
- `exercise/`: small orders API with public health and protected orders endpoints.
- `exercise/tests/`: deterministic xUnit tests for claims authorization and offline JWT validation with generated RSA keys.

## Exercise

See [`exercise/`](exercise/README.md): protect an Orders API and require the
`orders.read` scope or role.
