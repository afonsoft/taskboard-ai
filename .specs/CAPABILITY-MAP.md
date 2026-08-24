# Capability Map — Clone do Codex Taskboard em C# .NET 10

Este mapa é a Fase 0 do fluxo Spec-Driven Development. Ele decompõe o
sistema `dashi-taskboard` (fonte) em módulos de capacidade independentemente
testáveis, define a direção de dependência e a ordem de construção para o
clone em C# .NET 10 com servidor MCP e Skills.

Fonte analisada: `/home/ubuntu/repos/dashi-taskboard` (Node.js ESM, `node:sqlite`,
servidor HTTP customizado, React/Vite, Tauri).

## Módulos

| Module id | Responsabilidade | Depende de |
|---|---|---|
| domain-model | Entidades, enums, regras de estado (status/priority), modelo de concorrência otimista | — |
| persistence | Armazenamento SQLite, migrações, índices, integridade referencial, anexos em disco | domain-model |
| rest-api | Servidor HTTP, roteamento manual `/api/*`, auth por token de instância, CORS, SSE (EventHub), concorrência 409 | domain-model, persistence |
| cli | Cliente de linha de comando `taskctl` que fala HTTP com o serviço | rest-api |
| mcp | Servidor MCP (Model Context Protocol) que expõe operações como Tools, delegando no CLI/HTTP | cli, rest-api |
| ai-chat | Subsystem de chat AI local: threads, runs, eventos SSE por thread, spawn do Codex app-server | rest-api, persistence |
| cloud | Modo nuvem (companion loopback + proxy Cloudflare D1/R2), Basic Auth, polling de revisão | rest-api, persistence |
| workflow-automation | Motor de grafo de workflow (control-flow), automação de auto-claim via Codex | domain-model, rest-api |
| frontend | UI React (Vite) servida estaticamente; consumo da REST API + SSE | rest-api |
| skill | Skill `manage-taskboard` (markdown + referências) e skill de automação Codex | rest-api, cli |
| integrations | Jira (connection/sync), DeepSeek harness | rest-api, persistence |

Ordem de construção:
`domain-model` → `persistence` → `rest-api` → `cli` → `mcp`
→ `ai-chat` → `cloud` → `workflow-automation` → `skill` → (`frontend`, `integrations`).

Regra: setas apontam numa direção; `workflow-automation` e `integrations`
não se referenciam ciclicamente.

## Assumptions (a confirmar com o usuário)

1. O clone backend roda em **ASP.NET Core 10** (Minimal APIs ou controller-based),
   usando **SQLite** via **EF Core 10** ou **Microsoft.Data.Sqlite** (escolha a definir).
2. O servidor **MCP** usa o pacote oficial **ModelContextProtocol for .NET** (`ModelContextProtocol`),
   transport `Stdio`, espelhando os 13 tools atuais.
3. **Skills**: a skill `manage-taskboard` permanece em formato markdown (AGENTS/SKILL.md),
   portada 1:1 em conteúdo; a invocação em .NET fica a cargo do host de agente (OpenCode/Claude/etc.).
4. **Frontend**: o clone pode reutilizar o app React existente apontando para a nova API,
   OU reimplementar. Decisão fora do escopo inicial do backend — especificado mas não construído primeiro.
5. **Cloud**: Cloudflare D1/R2 é opcional no clone; o modo companion local é o alvo MVP.
6. Idioma dos docs/specs: **Português** (comunicação) / identificadores e enums mantêm
   os nomes originais em inglês do sistema-fonte para garantir paridade da API.

## Índice de specs (`.specs/`)

| Arquivo | Módulo | Conteúdo |
|---|---|---|
| SPEC-000-overview.md | — | Arquitetura-alvo, stack, comandos, sucesso |
| SPEC-001-domain-model.md | domain-model, persistence | Tabelas, campos, relacionamentos, enums |
| SPEC-002-rest-api.md | rest-api | Roteamento, endpoints, SSE, auth, 409 |
| SPEC-003-cli.md | cli | Comandos, flags, transporte HTTP |
| SPEC-004-mcp.md | mcp | Tools MCP, atribuição de thread |
| SPEC-005-ai-chat.md | ai-chat | Threads, runs, eventos, Codex app-server |
| SPEC-006-cloud.md | cloud | Companion, proxy, Basic Auth, polling |
| SPEC-007-workflow-automation.md | workflow-automation | Grafo, condition branches, automação |
| SPEC-008-frontend.md | frontend | Estrutura React, stores, SSE client |
| SPEC-009-skill.md | skill | manage-taskboard, automação, fluxo |
| SPEC-010-integrations.md | integrations | Jira, DeepSeek |

> Cada spec descreve o **comportamento atual do sistema-fonte** (fonte de verdade
> para o clone) e aponta o mapeamento para .NET 10.
