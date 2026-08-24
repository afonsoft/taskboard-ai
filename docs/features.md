# Features

## Core Board

- Projects and tasks with statuses, priorities, labels
- Kanban board sorted by `sort_order`
- Comments, attachments, task relations, activities
- Markdown GFM + mermaid support (read-only)
- Optimistic concurrency with `version` column

## Real-time

- Global SSE stream: `/api/events`
- Per-thread AI chat SSE: `/api/local/ai/threads/:id/events`
- Polling fallback for cloud mode (`GET /api/meta`)

## Automation

- Workflow workspaces (JSON board config)
- Control-flow engine
- Auto-claim `todo` → `in_progress` for Codex agents

## CLI

`taskctl` — System.CommandLine console with subcommands:
- `project`, `issue`, `comment`, `attachment`, `label`, `ai`, `context`, `search`
- JSON output via `--json`

## MCP Server

13 tools exposing project/issue/comment/attachment/label/search operations.

## AI Chat

- Threads, runs, events
- Model catalog
- Composer candidates / rebind
- Provider abstraction (OpenAI, Claude, Azure OpenAI)

## Cloud

- Local companion loopback
- Cloudflare D1/R2 proxy
- Basic Auth

## Integrations

- Jira sync
- DeepSeek harness
- Execution helpers (`CodexExecutableResolver`, `ProcessTreeSignaler`, `ExecutableCommand`)

See `.specs/SPEC-*.md` for full requirements.
