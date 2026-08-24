# Funcionalidades

## Board Principal

- Projetos e tarefas com status, prioridades, labels
- Board Kanban ordenado por `sort_order`
- Comentários, anexos, relações e atividades de tarefas
- Suporte a Markdown GFM + mermaid (read-only)
- Concorrência otimista com coluna `version`

## Tempo Real

- Stream SSE global: `/api/events`
- SSE por thread de IA: `/api/local/ai/threads/:id/events`
- Polling fallback para modo cloud (`GET /api/meta`)

## Automação

- Workspaces de workflow (config JSON do board)
- Engine de control-flow
- Auto-claim de `todo` → `in_progress` para agentes Codex

## CLI

`taskctl` — console System.CommandLine com subcomandos:
- `project`, `issue`, `comment`, `attachment`, `label`, `ai`, `context`, `search`
- Saída JSON via `--json`

## MCP Server

13 tools expondo operações de projeto/issue/comentário/anexo/label/busca.

## AI Chat

- Threads, runs, events
- Catalog de modelos
- Composer candidates / rebind
- Abstração de provider (OpenAI, Claude, Azure OpenAI)

## Cloud

- Companion loopback local
- Proxy Cloudflare D1/R2
- Basic Auth

## Integrações

- Sincronização Jira
- Harness DeepSeek
- Helpers de execução (`CodexExecutableResolver`, `ProcessTreeSignaler`, `ExecutableCommand`)

Veja `.specs/SPEC-*.md` para requisitos completos.
