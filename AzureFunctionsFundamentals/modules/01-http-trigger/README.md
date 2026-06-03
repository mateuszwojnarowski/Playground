# Module 01: HTTP Trigger

## Concept
HTTP-triggered Azure Functions expose small API endpoints. In .NET isolated worker apps, the ASP.NET Core integration package (`Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore`) lets functions use familiar `HttpRequest`, `IActionResult`, and JSON response types.

## Why it matters
HTTP triggers are often the front door for serverless systems: webhooks, lightweight APIs, command endpoints, and integration callbacks. Keeping the function as a thin adapter makes request handling easy to test without starting the Functions runtime.

## Scenario
You are building a minimal order intake endpoint. `POST /api/orders` accepts an `Order`, validates it, and returns either:

- `400 Bad Request` with validation errors.
- `201 Created` with the accepted order resource.

The validation and creation logic lives in injectable services so unit tests do not depend on Azure Functions or live emulators.

## Acceptance criteria
- [x] Use .NET 10 isolated worker and Azure Functions v4.
- [x] Use ASP.NET Core HTTP integration package version `2.1.0`.
- [x] Include a runnable `examples/` project with ping and hello endpoints.
- [x] Include a runnable `exercise/` project with `POST /api/orders`.
- [x] Put validation and creation logic in plain service classes.
- [x] Unit-test valid and invalid order cases without the Functions runtime.

## Run it locally

From `examples/`:

```bash
dotnet build
func start
curl http://localhost:7071/api/ping
curl "http://localhost:7071/api/hello?name=Ada"
```

From `exercise/`:

```bash
dotnet build
func start
curl -i -X POST http://localhost:7071/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":42,"product":"Keyboard","quantity":2,"unitPrice":49.99}'

curl -i -X POST http://localhost:7071/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":0,"product":"","quantity":0,"unitPrice":-1}'
```

## Tests

From `exercise/tests/`:

```bash
dotnet test
```

The tests cover successful order creation and multiple validation failures.
