# SPEC-006: Servidor MCP

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Servidor MCP (Model Context Protocol) |
| Product / System | dashi-taskboard |
| Module / Bounded Context | MCP |
| Change type | Migration |
| Repository | afonsoft/dashi-taskboard |
| Suggested branch | devin/spec-mcp-net10 |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O MCP server atual `mcp/index.mjs` expõe 13 tools via STDIO/SSE usando `@modelcontextprotocol/sdk`. Precisa de equivalente em .NET 10.

### Objective

Criar servidor MCP em C# expondo as mesmas tools: `list_projects`, `get_project`, `create_project`, `list_issues`, `get_issue`, `create_issue`, `update_issue`, `move_issue`, `archive_issue`, `add_comment`, `upload_attachment`.

### Expected outcome

Aplicação `Taskboard.Mcp` compatível com protocolo MCP, transporte STDIO e/ou SSE.

### Out of scope

- Ferramentas de IA generativa (ver SPEC-011).

---

## 2. Agent Role

> Senior .NET engineer com experiência em protocolos e servidores MCP.

---

## 3. Agent Autonomy Level

3

---

## 4. Product Context

### Functional context

MCP permite que IDEs (Claude, Cursor, VS Code Copilot, Devin, etc.) interajam com o Taskboard via tools padronizadas.

### Technical context

- `mcp/index.mjs` inicia servidor e delega para `taskctl` via `spawnTaskctl`.
- Tools schemas definidos em `toolSchemas`.
- Respostas JSON.

### Relevant stack

- .NET 10
- `ModelContextProtocol` SDK .NET (ou implementação manual STDIO/SSE)
- `System.Text.Json`
- `Taskboard.Application.Contracts`

---

## 5. Task Definition

### Main task

Implementar servidor MCP .NET com as tools existentes.

### Subtasks

- Implementar transporte STDIO.
- Mapear cada tool para command/query do `Application`.
- Validar inputs com schemas JSON.
- Retornar erros formatados.

### Do not do

- Não duplicar lógica de negócio (usar MediatR).

---

## 6. Functional Requirements

### FR-001: list_projects

**Description:** Lista projetos.  
**Input:** `{}` (opcional filtros).  
**Output:** `[{id,name,issueCount}]`.

### FR-002: get_project

**Input:** `{ "project_id": "local" }`.  
**Output:** `{ project }`.

### FR-003: create_project

**Input:** `{ "id", "name", "workspace_path" }`.  
**Output:** `{ project }`.

### FR-004: list_issues

**Input:** `{ "project_id", "status", "limit" }`.  
**Output:** `{ project, tasks }`.

### FR-005: get_issue

**Input:** `{ "issue_id" }`.  
**Output:** `{ task }`.

### FR-006: create_issue

**Input:** `{ "project_id", "title", "description", "status", "priority", "labels", "due_date" }`.  
**Output:** `{ task }`.

### FR-007: update_issue

**Input:** `{ "issue_id", "version", "changes" }`.  
**Output:** `{ task }`.

### FR-008: move_issue

**Input:** `{ "issue_id", "version", "status", "sort_order" }`.  
**Output:** `{ task }`.

### FR-009: archive_issue

**Input:** `{ "issue_id", "version" }`.  
**Output:** `{ task }`.

### FR-010: add_comment

**Input:** `{ "issue_id", "body" }`.  
**Output:** `{ comment }`.

### FR-011: upload_attachment

**Input:** `{ "issue_id", "file_path" }` (caminho local lido pelo servidor).  
**Output:** `{ attachment }`.

---

## 7. Business Rules

- O MCP apenas orquestra; lógica em Application/Domain.
- Schemas devem ser compatíveis com MCP 2024-11-05.
- Erros retornam `content` com texto descritivo.

---

## 8. Domain Modeling

Ver SPEC-002.

---

## 9. Expected Architecture

`Taskboard.Mcp` console app. `McpServerBuilder` com `AddTool` por tool. Injeta `IMediator`.

---

## 10. API Contracts

Tools schemas JSON (compatíveis com MCP).

---

## 11. Application Contracts

Reutiliza commands/queries de `Taskboard.Application`.

---

## 12. Persistence and Data

Ver SPEC-009.

---

## 13. Integrations

Taskboard HTTP/API interna (via DI).

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Tool inexistente | name inválido | erro protocolo |
| Parâmetro faltando | create sem title | erro de validação |
| Versão conflito | update com version antiga | retornar erro amigável |

---

## 15. Few-Shot Examples

```json
// Request
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": { "name": "create_issue", "arguments": { "project_id": "local", "title": "Bug", "status": "todo" } }
}
```

---

## 16-24. Standard SSD sections

---

## Pending Questions

1. Usar SDK oficial `ModelContextProtocol` .NET ou implementação manual?
2. Transporte padrão é STDIO ou SSE?

## Human Approval Checklist

Seguir template padrão SSD.
