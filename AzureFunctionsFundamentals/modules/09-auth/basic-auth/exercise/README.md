# Exercise - Protect a Reports API with Basic auth

## Scenario

You run a small Reports API as Azure Functions (isolated worker). A status probe
must stay public, but report data is only for known users who present a username
and password over HTTPS.

## Endpoints

- `GET /api/health` - public, returns `{ "status": "ok" }`.
- `GET /api/reports` - protected with HTTP Basic auth.

## Acceptance criteria

- [ ] Function project targets `net10.0`, Azure Functions v4, isolated worker, `OutputType` `Exe`.
- [ ] `GET /api/health` works without credentials.
- [ ] `GET /api/reports` returns `401` when the `Authorization` header is missing, malformed, names an unknown user, or carries the wrong password.
- [ ] A `401` response includes a `WWW-Authenticate: Basic` header.
- [ ] `GET /api/reports` returns the reports when the credentials are valid.
- [ ] Credential checking lives in `BasicAuthenticator`, a testable class independent of the Functions runtime, and uses a fixed-time password comparison.
- [ ] Tests are deterministic and offline.

## Run locally

```bash
func start --script-root modules/09-auth/basic-auth/exercise
```

```bash
curl http://localhost:7071/api/health
curl -u alice:correct-horse-battery-staple http://localhost:7071/api/reports
```

Users are configured under `BasicAuth:Users:<name>` in `local.settings.json`. In
Azure, store them as app settings (or, better, look them up from a secret store /
hashed credential store).

## Tests

Run from the `AzureFunctionsFundamentals` folder:

```bash
dotnet test modules/09-auth/basic-auth/exercise/tests/BasicAuthExercise.Tests.csproj
```

The tests build `BasicAuthenticator` with an in-memory credential store, so they
need no network or live Functions host.
