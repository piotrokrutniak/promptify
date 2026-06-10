# Promptify

Interview assignment: submit prompts, track processing status, view results.

**Stack:** .NET 10 Clean Architecture backend, PostgreSQL, RabbitMQ + MassTransit, separate Worker process, SignalR.

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

API at `http://localhost:8080/scalar`.

## LLM provider (Worker)

Default is `Mock` (no external calls). To use a real model, set `Llm:Provider` on the **Worker** process:

**OpenAI**

```bash
# Worker user secrets (Aspire local dev)
dotnet user-secrets set "Llm:Provider" "OpenAI" --project backend/src/Worker
dotnet user-secrets set "Llm:OpenAiApiKey" "sk-..." --project backend/src/Worker
```

**Ollama (local)**

```bash
ollama pull llama3.2
dotnet user-secrets set "Llm:Provider" "Ollama" --project backend/src/Worker
```

**Docker** — set worker env vars, e.g. `Llm__Provider=OpenAI` and `Llm__OpenAiApiKey=sk-...` (use a `.env` file; do not commit secrets).

See [docs/07-api-worker-integration.md](docs/07-api-worker-integration.md) for the full config table.

## Documentation

See [docs/README.md](docs/README.md) for phased development plan and requirements.
