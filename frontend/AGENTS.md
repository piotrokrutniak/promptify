<!-- BEGIN:nextjs-agent-rules -->
# This is NOT the Next.js you know

This version has breaking changes — APIs, conventions, and file structure may all differ from your training data. Read the relevant guide in `node_modules/next/dist/docs/` before writing any code. Heed deprecation notices.
<!-- END:nextjs-agent-rules -->


# Frontend agent guide

**Status:** not yet scaffolded (phase 05 deferred). This file defines the workflow to follow when `frontend/` is created.

Before substantial work, review `../docs/00-requirements.md`, `../docs/05-frontend.md`, and `../docs/07-api-worker-integration.md`.

# Product context

Promptify is a prompt submission and status-tracking system:

- Users create **sessions** (grouped conversations) and send prompts within them
- A separate **worker** processes prompts via a mock LLM
- Status updates flow back via **SignalR** (`/hubs/prompts`)
- One in-flight prompt per session (API enforces idle gate)

# Intended stack

- **Framework:** Next.js App Router, TypeScript
- **Styling:** Tailwind v4 + shadcn (`base-nova`)
- **Structure:** `features/prompts/` with atomic design (`atoms/`, `molecules/`, `organisms/`)
- **API client:** OpenAPI spec from backend → `@openapitools/openapi-generator-cli` → `typescript-fetch`
- **Real-time:** `@microsoft/signalr` hook on `PromptStatusHub`

# Backend + frontend API workflow

When a task spans backend and frontend:

1. **Backend first** — finish domain, application commands/queries, and endpoints in `../backend/`.
2. **Regenerate the contract** — with the backend running (`make apphost`), run `npm run generate:api-spec` from `frontend/`. This refreshes `openapi/v1.json` and `generated/api/`.
3. **Frontend second** — implement UI and `lib/api-client.ts` against the regenerated spec.

**Source of truth**

- `openapi/v1.json` and `generated/api/` define the API contract between layers.
- **Do not hand-edit** anything under `generated/api/`.
- Hand-written fetch helpers in `lib/api-client.ts` must stay aligned with the spec (paths, methods, request/response shapes). Prefer mirroring generated model names and fields.

If backend endpoints or DTOs change, stop and regenerate before continuing frontend work.

# API spec generation setup

When scaffolding `frontend/`, add these files (adapted from wassup-web):

**`package.json` script:**

```json
"generate:api-spec": "node ./scripts/fetch-openapi.mjs && rm -rf generated/api && openapi-generator-cli generate --generator-key v1"
```

**`scripts/fetch-openapi.mjs`** — fetches from `https://localhost:<port>/openapi/v1.json` (use Aspire Web API port or `8080` for Docker).

**`openapitools.json`** — generator config:

```json
{
  "generator-cli": {
    "version": "7.12.0",
    "generators": {
      "v1": {
        "generatorName": "typescript-fetch",
        "inputSpec": "openapi/v1.json",
        "output": "generated/api",
        "additionalProperties": {
          "supportsES6": true,
          "typescriptThreePlus": true,
          "withSeparateModelsAndApi": true
        }
      }
    }
  }
}
```

# Component structure

- `components/ui/` — shadcn/Base UI primitives (design system; not product atoms).
- `features/<feature>/{atoms,molecules,organisms}/` — product UI by feature.
- `features/<feature>/index.ts` — barrel exports for organisms consumers should use.
- `lib/` — API clients, validations, SignalR hooks; no React.

**Layers**

- **Atoms** — small presentational pieces; no feature state or side effects.
- **Molecules** — compose atoms + `ui/` primitives; local layout and props only.
- **Organisms** — hooks, orchestration, server actions wiring; import from feature barrels in pages.

**Imports**

- Pages and cross-feature code: `@/features/prompts`.
- Avoid importing atoms/molecules from routes; keep internals inside the feature folder.

# Key API operations for UI

| Operation | Purpose |
|-----------|---------|
| `CreateSession` | Start a new session with the first prompt |
| `GetSessions` | List user's sessions |
| `GetSessionById` | Load session with all prompts |
| `CreatePrompt` | Send follow-up prompt (session must be idle) |
| `CancelPrompt` | Cancel a pending prompt |
| SignalR `JoinSession` + `PromptStatusChanged` | Real-time status updates |
