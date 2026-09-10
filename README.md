# taskboard-ai

A local-first, AI-native taskboard inspired by `dashi-taskboard`, rewritten in **C# 14 / .NET 10**.

> **Default language:** English (en-us). See [README.pt-br.md](README.pt-br.md) for the Portuguese version.

## Overview

`taskboard-ai` is a local-first issue board for developers and AI agents. It provides a SQLite-backed task system, REST API, SSE real-time events, a `taskctl` CLI, an MCP server, and AI chat integration — all implemented in .NET 10 with ABP N-Layer / DDD.

## Architecture

```text
src/
  Taskboard.Domain/            # Aggregates, entities, value objects, domain events
  Taskboard.Application.Contracts/  # DTOs, interfaces
  Taskboard.Application/       # Commands, queries, handlers (MediatR)
  Taskboard.EntityFrameworkCore/  # EF Core + SQLite + repositories
  Taskboard.Server/            # ASP.NET Core Minimal APIs + SSE
  Taskboard.Cli/               # taskctl CLI (System.CommandLine)
  Taskboard.Mcp/               # MCP server (ModelContextProtocol SDK)
  Taskboard.AiChat/            # AI chat threads/runs/events
  Taskboard.Workflow/          # Workflow workspaces + automation
  Taskboard.Cloud/             # Cloud companion + sync
  Taskboard.Integrations/      # Jira, GitHub, agent orchestration, execution helpers
  Taskboard.Maui/              # Optional desktop Blazor Hybrid
  Taskboard.Blazor/            # Blazor Server web UI
tests/
  Taskboard.Domain.Tests/
  Taskboard.Application.Tests/
  Taskboard.IntegrationTests/
```

## Quick Start

```bash
# Clone
git clone https://github.com/afonsoft/taskboard-ai.git
cd taskboard-ai

# Build
dotnet restore Taskboard.sln
dotnet build Taskboard.sln

# Run tests
dotnet test Taskboard.sln

# Run server
dotnet run --project src/Taskboard.Server

# Run CLI
dotnet run --project src/Taskboard.Cli -- --help
```

## Continuous Integration

GitHub Actions provide:

- Build and test in Release mode, format verification, an 80% line-coverage gate, and vulnerable-package checks.
- SonarCloud analysis when the `SONAR_TOKEN` secret is configured.
- CodeQL analysis for C# and GitHub Actions.
- Weekly NuGet and GitHub Actions updates through Dependabot.

## GitHub Kanban & AI Agents

Set `GITHUB_TOKEN` before starting the server:

```bash
export GITHUB_TOKEN=your-github-token
```

Open `/github-board` to view GitHub issues as a Kanban board. Drag an issue to
**In Progress**, select an installed agent, and follow execution in the
**Logs** tab. The supported CLIs are Devin, Claude, Codex, OpenCode, and
OpenHands. Real-time agent logs are streamed through the SignalR hub at
`/agent-log-hub`.

## Build Order

See [`.specs/CAPABILITY-MAP.md`](.specs/CAPABILITY-MAP.md).

1. `domain-model`
2. `persistence`
3. `rest-api`
4. `cli`
5. `mcp`, `ai-chat`, `cloud`, `workflow-automation`
6. `skill`, `frontend`, `integrations`

## Documentation

- [`docs/README.md`](docs/README.md) — System documentation
- [`docs/technologies.md`](docs/technologies.md) — Technologies and versions
- [`docs/packages.md`](docs/packages.md) — NuGet and NPM packages
- [`docs/plugins.md`](docs/plugins.md) — Plugins and integrations
- [`docs/features.md`](docs/features.md) — Features
- [`docs/api.md`](docs/api.md) — REST API and SSE
- [`docs/architecture/`](docs/architecture/) — Architecture diagrams (generated with the Archify skill)
  - [`docs/architecture/architecture.html`](docs/architecture/architecture.html) — Interactive system architecture diagram
- [`.specs/`](.specs/) — Specification-driven development (SDD) specs

## Agent Harness

- [`CLAUDE.md`](CLAUDE.md) — Agent single source of truth
- [`.claude/`](.claude/) — Claude Code / Devin CLI harness
- [`.devin/config.json`](.devin/config.json) — Devin CLI configuration
- [`.agent/skills/`](.agent/skills/) — Google Antigravity skills

The agent harness uses skills from [`afonsoft/skills`](https://github.com/afonsoft/skills):

```bash
npx skills add afonsoft/skills
```

This installs skills into `.claude/skills` and `.devin/skills`; `skills-lock.json`
records the locked skill sources.

## Contributing

- Create a feature branch from `main` or `develop`.
- Follow `.specs/` and the global rules in `.claude/rules/global-rules.md`.
- Ensure `dotnet build` and `dotnet test` pass.
- Open a Pull Request.

## License

MIT (to be confirmed — see `LICENSE` if present).
