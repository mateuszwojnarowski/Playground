# Module 02: Timer Trigger

## Concept
Timer-triggered Azure Functions run on NCRONTAB schedules. Azure Functions uses six fields, including seconds, so `0 */5 * * * *` means every five minutes at second zero.

## Why it matters
Scheduled functions are useful for cleanup, aggregation, reconciliation, and report generation. The function should only schedule and log; business rules should live in plain services that can be tested with a known clock.

## Scenario
You are building a daily order cleanup summary. The scheduled function finds stale orders using a `TimeProvider`, counts them, and reports their total value. Tests inject a fake clock so the behavior is deterministic.

## Acceptance criteria
- [x] Use .NET 10 isolated worker and Azure Functions v4.
- [x] Use Timer extension package version `4.3.1`.
- [x] Include a runnable `examples/` timer project using a six-field NCRONTAB schedule.
- [x] Include a runnable `exercise/` daily cleanup project.
- [x] Keep stale-order logic in a plain injectable service using `TimeProvider`.
- [x] Unit-test the service with a fake/known time.

## Run it locally

From `examples/`:

```bash
dotnet build
func start
```

The sample logs every five minutes with schedule `0 */5 * * * *`.

From `exercise/`:

```bash
dotnet build
func start
```

For local learning, the exercise runs daily with schedule `0 0 2 * * *` and logs the cleanup summary.

## Tests

From `exercise/tests/`:

```bash
dotnet test
```

The tests verify stale-order selection, totals, and the no-stale-orders case with a fixed clock.
