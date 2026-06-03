# Basic authentication

HTTP **Basic** authentication (RFC 7617) is the simplest scheme. The client sends
the username and password on **every** request in the `Authorization` header:

```text
Authorization: Basic base64(username:password)
```

The server decodes the header, splits on the first `:`, and checks the password
against a credential store.

## Why it is simple — and what that costs you

- The credentials are only **Base64 encoded, not encrypted**. Anyone who can read
  the request sees the password. Basic auth is only safe over **HTTPS**.
- The password is sent on every call, so a leak anywhere on the path (logs,
  proxies, browser history) exposes it.
- There is no built-in expiry, scopes, or "sign in once". The client must hold the
  raw password for as long as it calls the API.

Use Basic auth for quick internal tools, health probes behind a gateway, or
machine-to-machine calls on a trusted network — not for user-facing apps.

## How the sample validates a request

`BasicAuthenticator` is a plain, injectable class (so it is unit-testable without
the Functions runtime):

1. Read the `Authorization` header and require the `Basic` scheme.
2. Base64-decode it and split into `username` and `password`.
3. Look the user up in an `ICredentialStore`.
4. Compare the password using `CryptographicOperations.FixedTimeEquals` to avoid
   leaking information through response timing.
5. On success, return a `ClaimsPrincipal`; on failure, return an error and a
   `WWW-Authenticate: Basic` header so the client knows which scheme to use.

> **Storing passwords:** this sample compares against configured values to keep the
> focus on the scheme. A real system stores **salted password hashes** and verifies
> them — for example with ASP.NET Core Identity's `PasswordHasher<TUser>` — never
> plaintext.

## Run the example

```bash
func start --script-root modules/09-auth/basic-auth/examples
# or: dotnet run --project modules/09-auth/basic-auth/examples/BasicAuthExample.csproj
```

```bash
# 401 - no credentials
curl -i http://localhost:7071/api/whoami

# 200 - valid credentials (configured in local.settings.json)
curl -u alice:correct-horse-battery-staple http://localhost:7071/api/whoami
```

## Exercise

See [`exercise/`](exercise/README.md): protect a Reports API with Basic auth and make
the provided unit tests pass.
