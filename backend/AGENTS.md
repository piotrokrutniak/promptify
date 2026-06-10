# Backend agent guide

Promptify backend: .NET 10 Clean Architecture, CQRS via MediatR, PostgreSQL, RabbitMQ + MassTransit, SignalR.

Before substantial work, review `../docs/00-requirements.md` and `../docs/07-api-worker-integration.md`.

## Architecture

| Layer | Path | Responsibility |
|-------|------|----------------|
| Domain | `src/Domain/` | Entities, enums, domain events |
| Application | `src/Application/` | Commands, queries, validators, handlers, DTOs |
| Infrastructure | `src/Infrastructure/` | EF Core, Identity, messaging, LLM client |
| Web | `src/Web/` | HTTP endpoints, SignalR hub, OpenAPI |
| Worker | `src/Worker/` | MassTransit consumers |

One feature per folder under `Application/`, e.g. `Sessions/Commands/CreateSession/CreateSession.cs` containing command record, validator, and handler.

## Adding an endpoint checklist

1. **Domain** — entity changes (if any) + EF configuration in `Infrastructure/Data/Configurations/`
2. **Application** — `record XxxCommand` + `XxxCommandValidator` + `XxxCommandHandler`
3. **Authorization** — add `[Authorize]` on the command/query when the endpoint requires auth
4. **Endpoint** — static class in `Web/Endpoints/` implementing `IEndpointGroup`
5. **Handler method** — named static method (never anonymous lambdas); map via `groupBuilder.MapPost(HandlerName)`
6. **OpenAPI** — `[EndpointSummary]` + `[EndpointDescription]` on every handler
7. **Return type** — `TypedResults.*` matching HTTP semantics (`Created`, `Ok`, `NoContent`, etc.)
8. **Request DTO** — nested `record` in the endpoint class, or shared model in `Application/Common/Models/` if reused
9. **Tests** — validator unit tests, handler unit tests, functional tests via `TestApp.SendAsync`

## Validation

- Rules live in `*Validator.cs` using FluentValidation
- All requests pass through `ValidationBehaviour` in the MediatR pipeline → `400 Bad Request` on failure
- Write validator unit tests for every non-trivial rule (required fields, max lengths, ranges)
- Functional tests should assert `ValidationException` for invalid commands sent through the pipeline

## Testing requirements

| Level | Location | What to test |
|-------|----------|--------------|
| Unit — validator | `tests/Application.UnitTests/` | Edge cases, boundary values |
| Unit — handler | `tests/Application.UnitTests/` | Persistence, side effects (mocks for `IPublishEndpoint`, `IUser`) |
| Functional | `tests/Application.FunctionalTests/` | Full MediatR pipeline + real DB via Aspire TestAppHost |

Functional tests use `TestApp.SendAsync(command)` and `TestApp.RunAsDefaultUserAsync()` — not HTTP client calls.

Run all tests: `make test` from repo root.

## OpenAPI / frontend contract

- Spec served at `/openapi/v1.json` (Scalar UI at `/scalar`)
- `operationId` is derived from the handler method name — must be a named static method
- `ApiExceptionOperationTransformer` auto-adds `400` to all operations; `401`/`403` on authorized endpoints
- Request/response schemas come from handler parameter types and `TypedResults` return types
- After changing endpoints or DTOs: build backend, then regenerate frontend clients (see `../frontend/AGENTS.md`)

## Session and prompt rules

From `../docs/07-api-worker-integration.md`:

- **Session init** (`POST /api/Sessions`) — creates session + first prompt (`OrderIndex = 0`, status `Pending`), publishes `ProcessPromptCommand`
- **Follow-up prompt** (`POST /api/Sessions/{id}/prompts`) — session must be idle (no `Pending` or `Processing` prompts) → `409 Conflict` otherwise
- **Ownership** — `SessionAccess.GetOwnedSessionAsync` enforces user owns the session
- **Dispatch** — publish `ProcessPromptCommand` after DB commit (acceptable for v1; transactional outbox is a stretch goal)
- **Cancel** — `Pending` prompts only via `POST /api/Prompts/{id}/cancel`

## Endpoint reference

| Method | Route | Handler |
|--------|-------|---------|
| `POST` | `/api/Sessions` | `CreateSession` |
| `GET` | `/api/Sessions` | `GetSessions` |
| `GET` | `/api/Sessions/{sessionId}` | `GetSessionById` |
| `POST` | `/api/Sessions/{sessionId}/prompts` | `CreatePrompt` |
| `POST` | `/api/Prompts/{promptId}/cancel` | `CancelPrompt` |

SignalR hub: `/hubs/prompts` — `JoinSession(sessionId)`, event `PromptStatusChanged`.
