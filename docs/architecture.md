# Architecture

Promptify lets users submit multiple LLM prompts and track their processing status and results. The system consists of a Next.js frontend and two backend processes (Web API and Worker) sharing PostgreSQL and RabbitMQ.

## Overview

```
User (Next.js) ──REST──► Web API ──► PostgreSQL (prompts + status)
                              │
                              └── RabbitMQ ──► Worker ──► ILlmClient
                              │
                         SignalR ◄── status events
```

## Components

| Component | Role |
|-----------|------|
| **Frontend** (Next.js) | Auth, submit prompts, display status/results via SignalR |
| **Web API** | REST endpoints, session rules, publishes jobs, SignalR hub |
| **Worker** | Consumes jobs, claims prompt, calls `ILlmClient`, updates DB |
| **PostgreSQL** | Source of truth for sessions, prompts, status |
| **RabbitMQ** | Dispatch (`ProcessPromptCommand`) + status events (`PromptStatusChanged`) |

## Request flow

1. Client `POST` prompt → API inserts `Pending` in DB, publishes `ProcessPromptCommand` to RabbitMQ.
2. Worker claims the prompt → `Processing` → calls LLM → `Completed` or `Failed`.
3. Worker publishes `PromptStatusChanged` → Web API forwards via SignalR to the client.

**Prompt states:** `Pending` → `Processing` → `Completed` | `Failed`. Pending prompts can also be `Cancelled`.

The API enforces one in-flight prompt per session (no new prompt while another is `Pending` or `Processing`).

## API surface

| Method | Route |
|--------|-------|
| `POST` | `/api/Sessions` |
| `GET` | `/api/Sessions` |
| `GET` | `/api/Sessions/{sessionId}` |
| `POST` | `/api/Sessions/{sessionId}/prompts` |
| `POST` | `/api/Prompts/{promptId}/cancel` |

**SignalR:** hub at `/hubs/prompts` — `JoinSession(sessionId)`, event `PromptStatusChanged`.

OpenAPI spec: `/openapi/v1.json` (Scalar UI at `/scalar`).

## LLM providers

The Worker builds multi-turn chat history from prior **Completed** prompts in the same session, then calls `ILlmClient.CompleteAsync(messages)`.

Provider is selected via `Llm:Provider` on the **Worker** process:

| Provider | Implementation | Notes |
|----------|----------------|-------|
| `Mock` | `MockLlmClient` | Default; echoes last user message with configurable delay |
| `OpenAI` | `OpenAiLlmClient` | Official OpenAI SDK; requires `Llm:OpenAiApiKey` |
| `Ollama` | `OllamaLlmClient` | Requires `Llm:OllamaBaseUrl` + `Llm:OllamaModel`; URL must be reachable from Worker |

Configuration lives in `Worker/appsettings.json`, user secrets in local dev, or env overrides in Docker/production.

| Key | Default | Purpose |
|-----|---------|---------|
| `Llm:Provider` | `Mock` | Provider switch |
| `Llm:MockDelayMs` | `2000` | Mock latency (ms) |
| `Llm:OpenAiApiKey` | — | OpenAI API key (secret) |
| `Llm:OpenAiModel` | `gpt-4o-mini` | OpenAI model |
| `Llm:OpenAiBaseUrl` | — | Optional endpoint override (Azure OpenAI) |
| `Llm:OllamaBaseUrl` | `http://localhost:11434` | Ollama HTTP API base URL (Worker-reachable; tunnel if Ollama is on host localhost) |
| `Llm:OllamaModel` | `llama3.2` | Model name on the Ollama instance |
| `Llm:TimeoutSeconds` | `120` | HTTP timeout for Ollama |

Worker fails at startup when `Provider=OpenAI` and the API key is missing.
