# Phase 03 — Cleanup

**Status:** done

## Goal

Remove template sample code (Todo, Weather, Identity) so the solution is a clean slate for Promptify.

## Removed

- `Application/TodoLists/`, `TodoItems/`, `WeatherForecasts/`
- `Web/Endpoints/TodoLists.cs`, `TodoItems.cs`, `WeatherForecasts.cs`, `Users.cs`
- `Domain/Entities/TodoList.cs`, `TodoItem.cs`, sample events/enums/value objects
- `Infrastructure/Identity/`, EF Todo configs, Identity DI
- Related functional and unit tests

## Kept

- MediatR pipeline behaviours, `IEndpointGroup` discovery
- OpenAPI + Scalar
- EF interceptors, `BaseEntity`, domain events infrastructure
- AppHost, ServiceDefaults, test harness

## Done when

- `make test` green
- Scalar shows no Todo/Weather/Identity endpoints
