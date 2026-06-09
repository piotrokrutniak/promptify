# Promptify — Development Phases

| Phase | Doc | Status |
|-------|-----|--------|
| 00 | [Requirements](00-requirements.md) | done |
| 01 | [Repo foundation](01-repo-foundation.md) | done |
| 02 | [Makefile](02-makefile.md) | done |
| 03 | [Cleanup](03-cleanup.md) | done |
| 04 | [App development](04-app-development.md) | done |
| 05 | [Frontend](05-frontend.md) | deferred |
| 06 | [Orchestration](06-orchestration.md) | done |
| 07 | [API–Worker integration](07-api-worker-integration.md) | done |

## Architecture

```
┌─────────────┐     REST      ┌──────────┐     RabbitMQ     ┌──────────┐
│  Frontend   │ ────────────► │  Web API │ ◄──────────────► │  Worker  │
│  (deferred) │ ◄── SignalR ──│  + Hub   │                  │ consumer │
└─────────────┘               └────┬─────┘                  └────┬─────┘
                                   │                              │
                              ┌────▼─────┐                  ┌─────▼────┐
                              │ Postgres │                  │ Mock LLM │
                              └──────────┘                  └──────────┘
```
