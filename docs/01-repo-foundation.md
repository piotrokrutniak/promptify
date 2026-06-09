# Phase 01 — Repo Foundation

**Status:** done

## Goal

Establish the monorepo baseline with a Clean Architecture .NET backend.

## What exists

- `backend/` — [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) v10.8.0
- Target framework: **.NET 10** (`net10.0`, SDK `10.0.201`)
- Layers: Domain, Application, Infrastructure, Web, AppHost, ServiceDefaults, Shared
- PostgreSQL via Aspire AppHost
- OpenAPI + Scalar

## Monorepo layout

```
promptify/
├── README.md
├── Makefile
├── docs/
├── docker-compose.yml   # phase 06
└── backend/
```

`frontend/` will be added in phase 05 when started.

## Template install (reference)

```bash
dotnet new install Clean.Architecture.Solution.Template::10.8.0
dotnet new ca-sln -n PromptifyWebApi
```

## Done when

- Backend solution builds and runs via AppHost.
