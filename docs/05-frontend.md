# Phase 05 — Frontend

**Status:** deferred

## Goal

Next.js UI for submitting prompts and viewing statuses/results. Not implemented yet.

## Intended stack (when started)

- **Framework:** Next.js App Router, TypeScript
- **Styling:** Tailwind v4 + shadcn (`base-nova`)
- **Structure:** `features/prompts/` with atomic design (`atoms/`, `molecules/`, `organisms/`)
- **API client:** OpenAPI spec from backend → `@openapitools/openapi-generator-cli` → `typescript-fetch`
- **Real-time:** `@microsoft/signalr` hook on `PromptStatusHub`
- **Reference:** [wassup-web/frontend](https://github.com/) patterns

## UX (TBD)

- Job queue table vs chat-style bubbles vs grouped sessions
- Decision deferred until backend is stable

## Prerequisites

- Phase 04 complete (API + SignalR working)
- OpenAPI spec available at `/openapi/v1.json`

## No tasks yet

Do not create `frontend/` until this phase is explicitly started.
