# FDE-2: Attribution Calculation API

## Overview

This project implements the Attribution Calculation API for FDE-2, including:
- Attribution calculation logic
- Input validation
- Idempotency handling
- Status and warnings logic
- Unit tests

## Structure

- **Controllers/AttributionController.cs**: API endpoint for attribution calculation.
- **Services/AttributionService.cs**: Core attribution logic and idempotency.
- **Model/AttributionRequest.cs, AttributionResponse.cs, AttributionGroupRequest.cs, AttributionGroupContribution.cs**: Data models.
- **Model/AttributionValidator.cs**: Input validation logic.
- **Model/IdempotencyStore.cs**: In-memory idempotency store.
- **Tests/AttributionServiceTests.cs**: Unit tests for all main scenarios.

## How to Run

1. Open the solution in Visual Studio or use the .NET CLI.
2. Build the solution: `dotnet build`.
3. Run the API project: `dotnet run --project Schwab-FDE-WebAPI`.
4. Use a tool like Postman to POST to `/api/performance/attribution` with a valid `AttributionRequest` JSON body.

## How to Test

1. Run tests with the .NET CLI: `dotnet test` at the solution root or from the `Schwab-FDE-WebAPI.Test` project.
2. All tests should pass, covering valid, degraded, review required, invalid input, and idempotency scenarios.

## Assumptions

- The sum of group weights must be between 99 and 101.
- Fallback returns are used if the primary return is missing.
- Idempotency is handled in-memory for demonstration purposes.

## Copilot Usage

- GitHub Copilot was used to assist with code generation and refactoring.
- See `PROMPT_LOG.md` for a short list of prompts used and traceability.

## Contact

For questions, contact the development team.
