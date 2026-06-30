# Custom Agent / Reusable Instruction Artifact

This file contains a reusable instruction artifact that can be used to configure a custom coding agent or establish a consistent instruction set for AI assistants (e.g., GitHub Copilot, Copilot Chat, or custom agents).

## High-level instructions

- Project: Schwab-FDE-WebAPI (ASP.NET Core, .NET 8)
- Goal: Implement FDE-1 (Daily Return Calculation) and FDE-2 (Attribution Calculation) with clean validation, unit tests, and minimal external dependencies.
- Constraints:
  - Target .NET 8
  - No external network calls in unit tests
  - Idempotency may be implemented in-memory for demo

## Coding style and expectations

- Use descriptive names for controllers, services and models.
- Keep business logic in services; controllers should be thin.
- Validate inputs early and return clear error responses (400) with problem details.
- Provide unit tests covering success, validation failures, and edge cases.

## Example instruction to a custom agent

"Given the repository structure and .NET 8 target, generate or modify code to implement the DailyReturnService with validation and unit tests. Keep controllers minimal and include clear error messages using ProblemDetails. Use xUnit or MSTest for automated tests. When changing files, preserve existing behavior unless a bug is being fixed."

## Reuse

- Use this file as the basis for reproducible prompts or to configure a custom agent.
