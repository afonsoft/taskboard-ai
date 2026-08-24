# CAPABILITY-MAP

Global capability map for the **taskboard-ai** .NET 10 implementation.

This module order guarantees that lower layers are designed (and built) before upper layers consume them.

## Module Matrix

| Build order | Module id | Responsibility | Depends on |
|---|---|---|---|
| 1 | `domain-model` | Entities, enums, state rules (status/priority), optimistic concurrency model | — |
| 2 | `persistence` | SQLite storage, EF Core migrations, indexes, referential integrity, disk attachments | domain-model |
| 3 | `rest-api` | HTTP server, manual `/api/*` routing, instance-token auth, CORS, SSE EventHub, concurrency 409 | domain-model, persistence |
| 4 | `cli` | `taskctl` command-line client that talks HTTP to the service | rest-api |
| 5 | `mcp` | MCP server exposing tools mapped to the API/CLI | cli, rest-api |
| 6 | `ai-chat` | AI chat subsystem: threads, runs, per-thread SSE events, Codex app-server spawn | rest-api, persistence |
| 7 | `cloud` | Cloud mode: companion loopback + Cloudflare D1/R2 proxy, Basic Auth, review polling | rest-api, persistence |
| 8 | `workflow-automation` | Workflow graph engine (control-flow) and auto-claim via Codex | domain-model, rest-api |
| 9 | `skill` | `manage-taskboard` skill (markdown + references) and Codex automation skill | rest-api, cli |
| 10 | `frontend` | Blazor/.NET MAUI desktop UI rewritten to consume REST API + SSE | rest-api |
| 11 | `integrations` | Jira connection/sync and DeepSeek harness adapter | rest-api, persistence |

## Legend

- `domain-model` is the only layer with no upstream dependency.
- `persistence` is the only infrastructure layer allowed to know about disk paths and SQLite schema details.
- `rest-api` owns HTTP contracts and SSE semantics; all other modules consume it.
- `cli` and `mcp` are presentation/automation layers over `rest-api`.
- `ai-chat`, `cloud`, `workflow-automation`, `integrations` are vertical slices over `rest-api` + `persistence`.
- `frontend` and `skill` are user/agent-facing surfaces.

## Build order summary

```
domain-model
  → persistence
    → rest-api
      → cli
        → mcp
        → ai-chat
        → cloud
        → workflow-automation
      → skill
      → frontend
      → integrations
```

## .NET project mapping

| Module | Project | Target |
|---|---|---|
| domain-model | `src/Taskboard.Domain` | `net10.0` |
| persistence | `src/Taskboard.EntityFrameworkCore` | `net10.0` |
| rest-api | `src/Taskboard.Server` | `net10.0` |
| cli | `src/Taskboard.Cli` | `net10.0` |
| mcp | `src/Taskboard.Mcp` | `net10.0` |
| ai-chat | `src/Taskboard.AiChat` | `net10.0` |
| cloud | `src/Taskboard.Cloud` | `net10.0` |
| workflow-automation | `src/Taskboard.Workflow` | `net10.0` |
| skill | `skills/manage-taskboard/` | Markdown |
| frontend | `src/Taskboard.Blazor` + `src/Taskboard.Maui` | `net10.0` |
| integrations | `src/Taskboard.Integrations` | `net10.0` |

## Test project mapping

| Module | Test project |
|---|---|
| Domain | `tests/Taskboard.Tests.Unit` |
| Application | `tests/Taskboard.Tests.Unit` |
| REST API | `tests/Taskboard.Tests.Integration` |
| CLI | `tests/Taskboard.Tests.Integration` / `tests/Taskboard.Tests.Unit` |
| MCP | `tests/Taskboard.Tests.Integration` |

## Acceptance gates

- [ ] `Taskboard.Domain` compiles with `TreatWarningsAsErrors`.
- [ ] `Taskboard.EntityFrameworkCore` migration runs and seeds `local` project.
- [ ] `Taskboard.Server` starts and exposes `/health`.
- [ ] `taskctl project list --json` returns the `local` project.
- [ ] MCP `list_projects` tool returns the same JSON.
