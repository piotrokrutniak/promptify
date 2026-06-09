# Phase 06 — Orchestration

**Status:** done

## Goal

One-command reviewer run via Docker. No local .NET SDK required.

## Components

| File | Purpose |
|------|---------|
| `backend/Dockerfile` | Web API image (`sdk:10.0` / `aspnet:10.0`) |
| `backend/Dockerfile.worker` | Worker image |
| `docker-compose.yml` | `db` + `webapi` + `worker` |
| `Makefile` | `docker-up`, `docker-down`, `docker-down-volumes` |

## Services

| Service | Port | Image |
|---------|------|-------|
| `db` | 5432 | postgres:16 |
| `webapi` | 8080 | backend Dockerfile |
| `worker` | — | Dockerfile.worker |

No frontend service until phase 05.

## Reviewer quick-start

```bash
git clone <repo>
cd promptify
make docker-up
# open http://localhost:8080/scalar
```

## Local dev vs Docker

| Path | Needs |
|------|-------|
| `make docker-up` | Docker only |
| `make apphost` | .NET 10 SDK |

## Done when

- `make docker-up` starts db + api + worker
- Prompts process end-to-end in containers
