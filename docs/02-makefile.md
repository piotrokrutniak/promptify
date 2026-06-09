# Phase 02 — Makefile

**Status:** done

## Goal

Daily dev commands without remembering dotnet paths.

## Tasks

- [x] Create root `Makefile` with `apphost`, `watch`, `test`

## Targets

| Target | Command |
|--------|---------|
| `make apphost` | Run Aspire AppHost (Postgres + Web + Worker) |
| `make watch` | AppHost with hot reload |
| `make test` | `dotnet test` in backend |

Docker targets added in phase 06.

## Done when

- `make apphost` starts the stack
- `make test` passes
