# Spec: MCP Server (module `mcp`)

Descreve `mcp/index.mjs` — servidor MCP que expõe operações do Taskboard como
**Tools**, delegando via spawn do `taskctl` CLI. Fonte p/ o clone `Taskboard.Mcp`
usando o pacote oficial **ModelContextProtocol for .NET** (server `Stdio`).

## Comportamento atual
- `Server` (`@modelcontextprotocol/sdk`) com `capabilities.tools`. Transport
  `StdioServerTransport` (stdio).
- Cada tool registrado via `server.setRequestHandler("tools/call"|"tools/list")`.
- 13 tools (schema Zod `.strict()`). `executeTool` faz `spawnTaskctl([...args], {threadId})`
  → `node cli/taskctl.mjs ...`, captura stdout (JSON) / stderr, retorna como
  `content:[{type:"text", text}]` ou `isError:true`.
- **Thread attribution** (prioridade): `params.threadId` → env `TASKBOARD_THREAD_ID`
  → `CODEX_THREAD_ID` → `"default-thread-id"`. Setado como `TASKBOARD_THREAD_ID`
  no env do child.

## Tools MCP (paridade exigida)

| Tool | Parâmetros | Mapeia p/ taskctl |
|---|---|---|
| `list_projects` | — | `project list --json` |
| `get_project` | `id` | `project get <id> --json` |
| `create_project` | `name`, `id?`, `workspacePath?` | `project create --name ...` |
| `list_issues` | `project`, `status?`, `archived?`(`true`\|`false`\|`all`) | `issue list --project ...` |
| `get_issue` | `id` | `issue get <id> --json` |
| `create_issue` | `project,title,description?,status?,priority?,labels?,threadId?,gitBranch?,worktreePath?,worktreeBranch?` | `issue create ...` |
| `update_issue` | `id,title?,description?,status?,priority?,labels?,threadId?` | `issue update <id> ...` |
| `move_issue` | `id,status,todoin_progress\|in_review\|blocked\|done\|canceled,threadId?` | `issue move <id> --status ...` |
| `archive_issue` | `id,action(archive\|restore),threadId?` | `issue <action> <id> --json` |
| `add_comment` | `issueId,body,threadId?` | `comment add <issueId> --body ...` |
| `upload_attachment` | `target(task\|comment),targetId,filePath,contentType?,kind?(inline\|attachment)` | `attachment upload ...` |

Enums MCP: `status ∈ backlog|todo|in_progress|in_review|blocked|done|canceled`;
`priority ∈ none|urgent|high|medium|low`.

## Configuração de clientes (existente, manter compatível)
- **Claude Desktop**: `command: node`, `args: [mcp/index.mjs]`.
- **OpenCode**: `mcp.servers.codex-taskboard` → `command: npx`, `args:["mcp","--server","index.mjs"]`.
- **Cursor**: `command: node`, `args:[mcp/index.mjs]`.
O clone .NET deve produzir config equivalente apontando para `dotnet run --project Taskboard.Mcp`.

## .NET mapping (`Taskboard.Mcp`)
- Pacote `ModelContextProtocol` + `ModelContextProtocol.AspNetCore` (ou server stdio).
- `IMcpServer` com `ListTools`/`CallTool`; cada tool declarada com `AIFunction`
  (descrição + schema JSON gerado de um record C# tipado).
- `CallTool` → invoca `Taskboard.Cli` via `Process.Start` (ou chama a API HTTP
  direto via `Taskboard.Client` compartilhado) passando `TASKBOARD_THREAD_ID`
  no env. Recomendado: **chamar a API HTTP direto** (evita spawn de processo),
  mantendo o mesmo envelope/erro.
- Thread attribution na mesma ordem de prioridade.
- Transport `Stdio` (herda stdio do host de agente).
