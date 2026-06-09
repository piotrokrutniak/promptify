# Phase 04 — App Development

**Status:** done

## Goal

Build the assignment backend: prompts API, separate Worker, mock LLM, SignalR.

## 4a — Prompt domain + API

- `Domain/Entities/Prompt.cs`, `Domain/Enums/PromptStatus.cs`
- CQRS: `CreatePrompt`, `GetPrompts`, `GetPromptById`
- `Web/Endpoints/Prompts.cs`
- EF config + `DbSet<Prompt>`

## 4b — Worker

- `backend/src/Worker/` — separate Generic Host process
- `PromptProcessorHostedService` — claim Pending → Processing → LLM → Completed/Failed
- Registered in AppHost

## 4c — LLM (mock)

- `ILlmClient` in Application
- `MockLlmClient` in Infrastructure
- Config: `"Llm": { "Provider": "Mock", "MockDelayMs": 2000 }`

## 4d — SignalR

- `PromptStatusHub` at `/hubs/prompts`
- Worker fires `pg_notify` on status change
- `PromptStatusListenerService` in Web LISTENs and broadcasts via hub

## Done when

- POST prompt via Scalar → Worker processes → GET returns Completed with mock result
- SignalR hub emits `PromptStatusChanged`
