# Automated Tests

This repository includes automated unit tests located in the `Schwab-FDE-WebAPI.Test` project.

How to run tests:

1. From the solution root, run: `dotnet test`
2. Or run tests from Visual Studio Test Explorer.

Test coverage:
- Unit tests exercise the `DailyReturnService` and `AttributionService` logic including valid flows, validation failures, and edge cases.

If tests are missing, add tests under the `Schwab-FDE-WebAPI.Test` project and update this file.
