# taskboard-ai

Taskboard local-first e AI-native inspirado no `dashi-taskboard`, reescrito em **C# 14 / .NET 10**.

> **Idioma padrão:** Inglês (en-us). Veja a [README.md](README.md) para a versão em inglês.

## Visão Geral

O `taskboard-ai` é um quadro de tarefas local-first para desenvolvedores e agentes de IA. Oferece sistema de tarefas com SQLite, API REST, eventos em tempo real via SSE, CLI `taskctl`, servidor MCP e integração de chat com IA — tudo implementado em .NET 10 com ABP N-Layer / DDD.

## Arquitetura

```text
src/
  Taskboard.Domain/            # Agregados, entidades, value objects, domain events
  Taskboard.Application.Contracts/  # DTOs, interfaces
  Taskboard.Application/       # Commands, queries, handlers (MediatR)
  Taskboard.EntityFrameworkCore/  # EF Core + SQLite + repositórios
  Taskboard.Server/            # ASP.NET Core Minimal APIs + SSE
  Taskboard.Cli/               # CLI taskctl (System.CommandLine)
  Taskboard.Mcp/               # Servidor MCP (ModelContextProtocol SDK)
  Taskboard.AiChat/            # Threads/runs/events de IA
  Taskboard.Workflow/          # Workspaces e automação de workflow
  Taskboard.Cloud/             # Companion cloud e sync
  Taskboard.Integrations/      # Jira, DeepSeek, helpers de execução
  Taskboard.Maui/              # Desktop Blazor Hybrid (opcional)
  Taskboard.Blazor/            # Web UI futura (opcional)
tests/
  Taskboard.Domain.Tests/
  Taskboard.Application.Tests/
  Taskboard.IntegrationTests/
```

## Início Rápido

```bash
# Clone
git clone https://github.com/afonsoft/taskboard-ai.git
cd taskboard-ai

# Build
dotnet restore Taskboard.sln
dotnet build Taskboard.sln

# Testes
dotnet test Taskboard.sln

# Executar servidor
dotnet run --project src/Taskboard.Server

# Executar CLI
dotnet run --project src/Taskboard.Cli -- --help
```

## Ordem de Build

Veja [`.specs/CAPABILITY-MAP.md`](.specs/CAPABILITY-MAP.md).

1. `domain-model`
2. `persistence`
3. `rest-api`
4. `cli`
5. `mcp`, `ai-chat`, `cloud`, `workflow-automation`
6. `skill`, `frontend`, `integrations`

## Documentação

- [`docs/README.md`](docs/README.md) — Documentação do sistema
- [`docs/technologies.md`](docs/technologies.md) — Tecnologias e versões
- [`docs/packages.md`](docs/packages.md) — Pacotes NuGet e NPM
- [`docs/plugins.md`](docs/plugins.md) — Plugins e integrações
- [`docs/features.md`](docs/features.md) — Funcionalidades
- [`docs/api.md`](docs/api.md) — API REST e SSE
- [`.specs/`](.specs/) — Especificações SDD

## Agent Harness

- [`CLAUDE.md`](CLAUDE.md) — Fonte única de verdade para agentes
- [`.claude/`](.claude/) — Harness para Claude Code / Devin CLI
- [`.devin/config.json`](.devin/config.json) — Configuração do Devin CLI
- [`.agent/skills/`](.agent/skills/) — Skills para Google Antigravity

## Contribuição

- Crie uma branch a partir de `main` ou `develop`.
- Siga as `.specs/` e as regras globais em `.claude/rules/global-rules.md`.
- Garanta que `dotnet build` e `dotnet test` passem.
- Abra um Pull Request.

## Licença

MIT (a confirmar — veja `LICENSE` se existir).
