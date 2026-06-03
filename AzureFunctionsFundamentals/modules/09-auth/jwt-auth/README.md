# JWT (bearer token) authentication

A **JSON Web Token (JWT)** is a signed, self-contained token. Instead of sending a
password on every call (Basic auth), the client authenticates once and is given a
token to present on subsequent requests.

This module covers the **simplest** JWT setup: a **first-party** API that both
**issues** and **validates** its own tokens, signed with a **shared secret**
(HMAC-SHA256 / "HS256"). The client signs in once, receives a token, and then sends
it back on every call in the `Authorization` header using the `Bearer` scheme.
There is no external identity provider, no discovery document, and no public-key
download — the next module ([`oidc-oauth`](../oidc-oauth/README.md)) adds those.

## Anatomy of a JWT

`header.payload.signature`, each Base64Url-encoded:

- **Header** - algorithm (`alg`, e.g. `HS256`) and token type.
- **Payload** - claims: `sub` (subject/user id), `iss` (issuer), `aud` (audience),
  `exp` (expiry), `nbf` (not before), plus app claims such as `role`.
- **Signature** - HMAC of `header.payload` using the shared secret. It proves the
  token was issued by someone holding the secret and has not been altered.

The payload is only encoded, **not encrypted** — never put secrets in a JWT.

## What "validate a token" means

A protected endpoint must check **all** of these before trusting a token:

1. **Signature** - recompute the HMAC with the shared secret and compare.
2. **Issuer** (`iss`) - the token came from the issuer you trust.
3. **Audience** (`aud`) - the token was minted for *this* API.
4. **Lifetime** (`exp`/`nbf`) - the token is currently valid.
5. **Algorithm** - pin the expected algorithm so a token cannot downgrade to
   `none` or swap to a different scheme.
6. **Authorization claims** - the required `role`/`scope` is present for the
   operation.

`JwtTokenService` does steps 1-5 with `TokenValidationParameters`; the operation
decides step 6.

## Shared secret vs. public/private keys

HS256 uses **one shared secret** for both signing and verifying. That is fine when
the **same trust boundary** issues and consumes the token (a first-party API).
If a *separate* identity provider issues tokens for *many* APIs, you want
**asymmetric** keys (RS256): the provider signs with a private key and every API
verifies with the published public key. That is the OIDC/OAuth model in the next
module.

Keep the secret out of source control (use app settings / Key Vault) and rotate it.

## Run the example

```bash
func start --script-root modules/09-auth/jwt-auth/examples
```

```bash
# Get a token from the demo sign-in endpoint
TOKEN=$(curl -s -X POST http://localhost:7071/api/token \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","password":"let-me-in"}' | jq -r .access_token)

# Call the protected endpoint (curl sets the Authorization header for you)
curl --oauth2-bearer "$TOKEN" http://localhost:7071/api/protected
```

> The `/token` endpoint is a **demo** sign-in: real systems verify the user against
> an identity store (for example ASP.NET Core Identity) before issuing a token.

## Exercise

See [`exercise/`](exercise/README.md): protect a Documents API that requires a valid
JWT containing the `documents.read` role.
