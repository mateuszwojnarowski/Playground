# Exercise - Protect a Documents API with a JWT

## Scenario

You run a Documents API as Azure Functions (isolated worker). Clients sign in once,
receive a first-party JWT, and call the API with it. Document data requires the
`documents.read` role to be present in the token.

## Endpoints

- `GET /api/health` - public, returns `{ "status": "ok" }`.
- `GET /api/documents` - protected, requires a valid JWT containing the `documents.read` role.

## Acceptance criteria

- [ ] Function project targets `net10.0`, Azure Functions v4, isolated worker, `OutputType` `Exe`.
- [ ] `GET /api/health` works without a token.
- [ ] `GET /api/documents` returns `401` when the token is missing, malformed, expired, signed with the wrong key, from the wrong issuer, or for the wrong audience.
- [ ] `GET /api/documents` returns `403` when the token is valid but lacks the `documents.read` role.
- [ ] `GET /api/documents` returns the documents when the token is valid and carries `documents.read`.
- [ ] Token validation lives in `JwtTokenService` and the role check in `DocumentsAuthorizer`, both testable without the Functions runtime.
- [ ] The validator pins the HS256 algorithm and validates issuer, audience, lifetime, and signing key.
- [ ] Tests are deterministic and offline.

## Run locally

```bash
func start --script-root modules/09-auth/jwt-auth/exercise
```

The signing key, issuer, audience, and required role are configured under `Jwt:*`
in `local.settings.json`. Keep the signing key in app settings / Key Vault in Azure,
never in source control.

## Tests

Run from the `AzureFunctionsFundamentals` folder:

```bash
dotnet test modules/09-auth/jwt-auth/exercise/tests/JwtAuthExercise.Tests.csproj
```

The tests mint tokens in memory with `JwtTokenService` and assert the authorization
decisions, so they need no network or live Functions host.
