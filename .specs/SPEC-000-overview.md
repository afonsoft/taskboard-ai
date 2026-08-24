# Spec: Codex Taskboard Clone (C# .NET 10)

## Objective

Clonar o comportamento funcional do `dashi-taskboard` (local-first issue board
com integração a agentes de IA via CLI, MCP e Skills) para uma base **C# .NET 10**.
Esta spec é a visão de alto nível; cada subsistema tem sua própria spec em
`CAPABILITY-MAP.md`. O sistema-fonte é a fonte de verdade; o clone deve manter
paridade de **API HTTP, modelo de dados, comportamento de concorrência e
contrato MCP** para que o CLI, a Skill e os clientes React existentes continuem
funcionando sem mudança.

Usuário-alvo: engenheiros construindo o backend .NET e integrações de agente.
Sucesso = um serviço .NET 10 que responde à mesma superfície `/api/*`, persiste
em SQLite, emite SSE, e um servidor MCP .NET que expõe os mesmos 13 tools.

## Tech Stack (alvo)

- **Runtime**: .NET 10 (LTS), `net10.0`, C# 13.
- **Web**: ASP.NET Core 10 — Minimal APIs + `WebApplication` (sem framework pesado).
- **Persistência**: **`Microsoft.Data.Sqlite` (raw SQL)** — fiel ao `node:sqlite`
  do original; decisão confirmada. Migrações: script SQL idempotente espelhando
  `cloud/migrations/*.sql`. Lock otimista `version` via
  `UPDATE ... WHERE id=@id AND version=@version`.
- **MCP**: `ModelContextProtocol` (.NET, pacote oficial) — server `Stdio`, tools com `AIFunction`/schema JSON.
- **Anexos**: arquivos em disco sob `<dataDir>/attachments/<id>` (igual ao original).
- **SSE**: respostas `text/event-stream` nativas do ASP.NET Core.
- **Testes**: `xUnit` + `FluentAssertions`; testes de integração HTTP com `WebApplicationFactory`.
- **Build/CLI**: `dotnet` CLI; o `taskctl` vira um projeto console `Taskboard.Cli`.

## Commands

```bash
# Build e run do serviço
dotnet build src/Taskboard.Server
dotnet run --project src/Taskboard.Server

# CLI
dotnet run --project src/Taskboard.Cli -- project create --id my-project --name "My" --workspace-path /abs/path
dotnet run --project src/Taskboard.Cli -- issue list --project my-project --json

# MCP server (stdio)
dotnet run --project src/Taskboard.Mcp

# Testes
dotnet test

# Lint (editorconfig + analisadores)
dotnet build /warnaserror
```

Variáveis de ambiente (paridade com o original):
`CODEX_TASKBOARD_HOST` (default `0.0.0.0`), `CODEX_TASKBOARD_PORT` (47823),
`CODEX_TASKBOARD_DATA_DIR` (`.data`), `CODEX_TASKBOARD_URL`,
`TASKBOARD_THREAD_ID`, `CODEX_THREAD_ID`, `CODEX_TASKBOARD_COMPANION_URL`.

## Project Structure (alvo)

```
src/
  Taskboard.Domain/        → entidades, enums (TASK_STATUSES, TASK_PRIORITIES), regras
  Taskboard.Persistence/  → DbContext / SqliteConnection, migrações, repositórios
  Taskboard.Server/       → ASP.NET Core, Program.cs, endpoints /api/*, SSE, auth
  Taskboard.Cli/          → equivalente a taskctl (console)
  Taskboard.Mcp/          → servidor MCP (ModelContextProtocol)
  Taskboard.AiChat/       → subsystem de chat AI local (threads/runs/eventos)
  Taskboard.Cloud/        → companion loopback + proxy
  Taskboard.Workflow/     → motor de grafo de workflow + automação
  Taskboard.Integrations/ → Jira, DeepSeek
tests/
  Taskboard.Tests.Unit/
  Taskboard.Tests.Integration/   → WebApplicationFactory + CLI
skills/
  manage-taskboard/       → SKILL.md (portado 1:1) + references/cli.md
.specs/                   → este conjunto de specs
```

## Code Style

- `file-scoped namespace`, `primary constructors` (C# 12+), `collection expressions`.
- Injeção de dependência via `IServiceCollection`; handlers como `static`
  Minimal API ou classes `EndpointGroup`.
- Nomes de campos/rotas **exatos** do sistema-fonte (ex.: `identifier`,
  `sort_order`, `thread_id`, `/api/tasks`). Enums em inglês minúsculo.
- Exemplo (endpoint Minimal API):

```csharp
app.MapGet("/api/projects", (ProjectRepository repo) =>
    Results.Ok(new { projects = repo.ListProjects() }));

app.MapPost("/api/tasks", async (CreateTaskRequest body, TaskService svc) =>
{
    var task = await svc.CreateAsync(body);
    return Results.Created($"/api/tasks/{task.Id}", task);
});
```

- Tratamento de erro: `ApiError(code, message, details?)` → JSON no stderr (CLI)
  ou corpo HTTP `{ error: { code, message } }` (API). Códigos: `VERSION_CONFLICT` (409),
  `UNKNOWN_QUERY_PARAMETER` (400), `INVALID_CLOUD_URL`, etc.

## Testing Strategy

- **Unit**: regras de domínio (transições de status, validação de labels,
  `threadBinding`, concorrência `version`).
- **Integration**: `WebApplicationFactory` dispara a API real; cobre cada rota
  de `SPEC-002` com payloads exatos; verifica SSE e 409 em update concorrente.
- **CLI**: subprocess chama `Taskboard.Cli` e valida envelope JSON + exit codes.
- **MCP**: cliente in-process lista/invoca tools e compara com `SPEC-004`.
- Cobertura mínima: 80% dos paths de mutação de `tasks`/`comments`.

## Boundaries

- **Always**: manter paridade de contrato da API (nomes, campos, códigos de erro);
  usar `version` otimista em toda mutação de `tasks`/`comments`; persistir anexos
  em disco fora do DB; validar entrada (títulos ≤240, labels ≤20×64, etc.).
- **Ask first**: trocar SQLite por outro banco; mudar modelo de auth (LAN sem
  auth → decidir se mantém); adicionar dependências (ex.: EF Core vs raw);
  alterar a superfície MCP.
- **Never**: commitar secrets/`.data`; expor dados de device (paths absolutos)
  em modo cloud; quebrar compatibilidade da CLI/Skill existentes; gravar em dois
  bancos (local + cloud) simultaneamente.

## Success Criteria

- [ ] `GET /api/projects`, `POST /api/tasks`, `POST /api/tasks/:id/move`, etc.
  respondem com os mesmos corpos do original (IDs, campos computados, `version`).
- [ ] SQLite em `.data/taskboard.sqlite` com schema equivalente às 8+ tabelas.
- [ ] SSE em `/api/events` emite os 12 tipos de evento; `/api/local/ai/threads/:id/events` por thread.
- [ ] `Taskboard.Cli` replica os comandos de `SPEC-003` com os mesmos exit codes.
- [ ] `Taskboard.Mcp` expõe os 13 tools de `SPEC-004` via Stdio.
- [ ] Skill `manage-taskboard` funciona com o novo serviço sem alteração de conteúdo.
- [ ] Update concorrente retorna 409 `VERSION_CONFLICT` com `{expectedVersion, actualVersion}`.

## Open Questions (resolvidas)

1. **EF Core ou raw?** → **Raw `Microsoft.Data.Sqlite`** (paridade 1:1 das migrações e lock `version`).
2. **Cloud no MVP?** → **Só companion local loopback**; Cloudflare D1/R2 é deployment opcional posterior.
3. **Frontend?** → **Reescrito em Blazor (.NET MAUI para desktop)** — ver `SPEC-011-frontend-blazor.md`. App React original não é reutilizado.
4. **Workflow/automation no MVP?** → **Sim**, incluído (`SPEC-007`).
5. **Transporte MCP?** → **Chama a API HTTP direto** (não spawn do CLI) — confirmado em `SPEC-004`.
