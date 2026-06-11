# Promptify

Interview assignment: submit prompts, track processing status, view results.

**Stack:** .NET 10 backend, Next.js frontend, PostgreSQL, RabbitMQ + MassTransit, separate Worker process, SignalR.

## Quick start (local dev)

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
make apphost    # Aspire: Postgres + RabbitMQ + Web API + Worker
make test       # run tests
```

Open Scalar API docs from the Aspire dashboard, or the Web API URL + `/scalar`.

## Quick start (Docker — reviewers)

Requires Docker only.

```bash
make docker-up
```

- UI at `http://localhost:3000`
- API docs at `http://localhost:8080/scalar`

Sign in with the seeded account: `administrator@localhost.com` / `Administrator1!`

Mock LLM is enabled by default — no API keys required.

The frontend runs in Docker here for convenience; in production it would usually be deployed to Vercel (or similar) instead.

## LLM provider (Worker)

Default is `Mock` (no external calls). To use a real model, set `Llm:Provider` on the **Worker** process:

**OpenAI**

```bash
# Worker user secrets (Aspire local dev)
dotnet user-secrets set "Llm:Provider" "OpenAI" --project backend/src/Worker
dotnet user-secrets set "Llm:OpenAiApiKey" "sk-..." --project backend/src/Worker
```

**Ollama**

Set `Llm:Provider` to `Ollama`, plus `Llm:OllamaModel` (model name on your instance) and `Llm:OllamaBaseUrl` (HTTP API URL the **Worker** can reach). If Ollama runs on your host at `localhost:11434`, tunnel it (e.g. dev tunnels, ngrok) — the Worker in Aspire or Docker cannot reach your machine's localhost directly.

```bash
dotnet user-secrets set "Llm:Provider" "Ollama" --project backend/src/Worker
dotnet user-secrets set "Llm:OllamaModel" "llama3.2" --project backend/src/Worker
dotnet user-secrets set "Llm:OllamaBaseUrl" "https://your-tunnel.example/..." --project backend/src/Worker
```

User secrets load when the Worker runs in Development (`make apphost` or `dotnet run --project backend/src/Worker`). Docker uses `Llm__*` env vars instead, configured from `appsettings.json`.

**Docker** — set worker env vars, e.g. `Llm__Provider=OpenAI` and `Llm__OpenAiApiKey=sk-...` (use a `.env` file; do not commit secrets).

## Documentation

See [docs/architecture.md](docs/architecture.md) for system architecture.
