# FDE-1: Daily Return Calculation API

## Overview

This project implements the Daily Return Calculation API for FDE-1, including:
- Calculation of daily portfolio returns
- Input validation
- Status and warnings logic
- Unit tests

## Structure

- **Controllers/DailyReturnController.cs**: API endpoint for daily return calculation.
- **Services/DailyReturnService.cs**: Core calculation logic.
- **Model/DailyReturnRequest.cs, DailyReturnResponse.cs**: Data models.
- **Model/ValidationAssistant.cs**: Input validation logic.
- **Tests/DailyReturnServiceTests.cs**: Unit tests for all main scenarios.

## How to Run

1. Open the solution in Visual Studio.
2. Build the solution.
3. Run the API project (e.g., using IIS Express or Kestrel).
4. Use a tool like Postman to POST to `/api/performance/dailyreturn` with a valid `DailyReturnRequest` JSON body.

## How to Test

1. Open the Test Explorer in Visual Studio.
2. Run all tests in the `DailyReturnServiceTests` class.
3. All tests should pass, covering valid, invalid, and edge case scenarios.

## Assumptions

- Negative market values are not allowed.
- Currency is required.
- Begin market value of zero with a non-zero end market value is invalid.

## Copilot Usage

- GitHub Copilot was used to assist with code generation and refactoring.
- See prompt log comments in source files for traceability.

## Contact

For questions, contact the development team.
