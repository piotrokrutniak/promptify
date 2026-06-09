# Promptify

Interview assignment: submit prompts, track processing status, view results.

**Stack:** .NET 10 Clean Architecture backend, PostgreSQL, separate Worker process, SignalR.

## Quick start (local dev)

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
make apphost    # Aspire: Postgres + Web API + Worker
make test       # run tests
```

Open Scalar API docs from the Aspire dashboard, or the Web API URL + `/scalar`.

## Quick start (Docker — reviewers)

Requires Docker only.

```bash
make docker-up
```

API at `http://localhost:8080/scalar`.

## Documentation

See [docs/README.md](docs/README.md) for phased development plan and requirements.
