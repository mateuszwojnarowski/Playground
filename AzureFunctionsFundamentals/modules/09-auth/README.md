# Module 09 - Authentication & authorization

This module shows three ways to protect an Azure Functions (.NET 10 isolated worker)
HTTP API, from the simplest to the most capable. Each sub-module has a `README.md`
explaining the concept, a runnable `examples/` project, and an `exercise/` with
acceptance criteria and **deterministic, offline** unit tests.

Work through them in order — each builds on the previous one.

| # | Method | What the client sends | Who issues credentials | Signing | Best for |
|---|--------|-----------------------|------------------------|---------|----------|
| 1 | [Basic auth](basic-auth/README.md) | username + password on **every** request | you (a credential store) | none (Base64 only) | quick internal tools behind HTTPS |
| 2 | [JWT auth](jwt-auth/README.md) | a signed token (sign in once) | the **same** API (first-party) | shared secret (HS256) | first-party APIs that mint their own tokens |
| 3 | [OIDC / OAuth 2.0](oidc-oauth/README.md) | a signed token from an external provider | a **separate** authorization server | public/private keys (RS256) + JWKS | delegated access, many clients/APIs, real sign-in |

## How to choose

- **Basic auth** is the simplest to implement but sends the password on every call,
  so it must run over HTTPS and offers no expiry, scopes, or single sign-on. Good for
  internal or machine-to-machine calls on a trusted path.
- **JWT auth** lets a client sign in once and present a signed token afterwards. With
  a **shared secret** it is ideal when the *same* trust boundary both issues and
  validates tokens (a first-party API).
- **OIDC / OAuth 2.0** delegates authentication to a dedicated **authorization
  server** that signs tokens with a private key; APIs validate using the published
  public keys (JWKS) discovered from `/.well-known/openid-configuration`. This scales
  to many clients and APIs, supports real user sign-in and consent, and is the
  standard for production. This module uses **ASP.NET Core Identity** (optionally
  fronted by the open-source OpenIddict server) as the provider.

## Concepts shared across all three

- The `Authorization` HTTP header carries the credential; the first word is the
  **scheme** (`Basic`, `Bearer`).
- Keep credential/token validation in a **plain, injectable class** so you can unit
  test it without the Functions runtime or a live identity provider (see the
  course [`CONVENTIONS.md`](../../CONVENTIONS.md) testability rule).
- **Authentication** answers "who is calling?"; **authorization** answers "are they
  allowed to do this?" The exercises separate the two so each is testable.

## Build & test this module

```bash
cd AzureFunctionsFundamentals
dotnet test modules/09-auth/basic-auth/exercise/tests/BasicAuthExercise.Tests.csproj
dotnet test modules/09-auth/jwt-auth/exercise/tests/JwtAuthExercise.Tests.csproj
dotnet test modules/09-auth/oidc-oauth/exercise/tests/OidcOAuthExercise.Tests.csproj
```
