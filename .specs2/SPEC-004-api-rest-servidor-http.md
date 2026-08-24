# SPEC-004: API REST e Servidor HTTP

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | API REST e Servidor HTTP |
| Product / System | dashi-taskboard |
| Module / Bounded Context | Taskboard HTTP API |
| Change type | Migration |
| Repository | afonsoft/dashi-taskboard |
| Suggested branch | devin/spec-httpapi-net10 |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O servidor atual `server/app.mjs` (3.300+ linhas) usa `node:http` raw e roteia manualmente para health, storage local, projetos, tarefas, eventos SSE, anexos, comentários, workflow, AI e cloud/Jira.

### Objective

Mapear todos os endpoints HTTP do Node.js para ASP.NET Core Minimal APIs / controllers, preservando rotas, métodos, query params, payloads e códigos de erro.

### Expected outcome

Aplicação `Taskboard.HttpApi.Host` expondo todos os endpoints com ProblemDetails.

### Out of scope

- Implementação do client frontend (ver SPEC-008).
- Deploy Cloudflare (ver SPEC-010).

---

## 2. Agent Role

> Senior ASP.NET Core engineer com experiência em Minimal APIs, SSE, file streaming e ProblemDetails.

---

## 3. Agent Autonomy Level

3

---

## 4. Product Context

### Functional context

Servidor local-first escutando em `0.0.0.0`/`127.0.0.1` na porta `CODEX_TASKBOARD_PORT` (default 47823). Serve a SPA estática e expõe API JSON.

### Technical context

- Raw `node:http`.
- Roteamento manual por `pathname` e regex.
- SSE em `/api/events`.
- Anexos como arquivos binários.
- Erros com `{ error: { code, message, details? } }`.

### Relevant stack

- ASP.NET Core .NET 10
- Minimal APIs
- Server-Sent Events (SSE)
- ProblemDetails

---

## 5. Task Definition

### Main task

Implementar todos os endpoints HTTP equivalentes.

### Subtasks

- Health e meta.
- Client storage e local endpoints.
- Projetos (`/api/projects`).
- Tarefas (`/api/tasks`).
- Comentários e anexos.
- Relacionamentos e atividades.
- Workflow workspaces e device workspaces.
- AI catalog/threads/composer.
- Jira connection.
- SSE events.
- Static files fallback.

### Do not do

- Não reimplementar lógica de IA sem SPEC-011.
- Não reimplementar Jira sem SPEC-010.

---

## 6. Functional Requirements

### FR-001: Health

**Endpoint:** `GET /health`  
**Response:** `200 OK` com status e metadata.

### FR-002: Projetos

**Endpoints:**

```http
GET    /api/projects
POST   /api/projects
```

**Regras:**

- GET lista todos com `issueCount`.
- POST cria com id, name, workspacePath.
- Projeto `local` sempre inicializado.

### FR-003: Tarefas

**Endpoints:**

```http
GET    /api/tasks?projectId=&status=&archived=&...         # listar
POST   /api/tasks                                         # criar
GET    /api/tasks/{id}
PATCH  /api/tasks/{id}
DELETE /api/tasks/{id}
POST   /api/tasks/{id}/move
POST   /api/tasks/{id}/archive
POST   /api/tasks/{id}/restore
GET    /api/tasks/{id}/comments
POST   /api/tasks/{id}/comments
PATCH  /api/tasks/{taskId}/comments/{commentId}
DELETE /api/tasks/{taskId}/comments/{commentId}
GET    /api/tasks/{id}/activities
GET    /api/tasks/{id}/relations
POST   /api/tasks/{id}/relations
DELETE /api/tasks/{id}/relations/{type}/{targetTaskId}
```

**Regras:**

- Query params para listagem: projectId, status, q, assigneeId, label, archived, etc.
- PATCH requer `version` e `changes`.
- DELETE requer `version` e a tarefa deve estar arquivada e não ser Jira.

### FR-004: Anexos

**Endpoints:**

```http
POST   /api/attachments
GET    /api/attachments/{id}/content
GET    /api/attachments/{id}/download
DELETE /api/attachments/{id}
```

### FR-005: SSE Events

**Endpoint:** `GET /api/events`  
Emite eventos: `task.created`, `task.updated`, `task.moved`, `task.archived`, `task.restored`, `task.deleted`, `comment.added`, `attachment.deleted`.

### FR-006: Local endpoints

```http
GET/PUT  /api/client-storage
GET      /api/local/codex-thread-progress
GET      /api/local/host-runtime
GET/PUT  /api/local/cloud-session
GET/POST /api/local/jira-connection
POST     /api/local/jira-connection/sync
GET      /api/meta
GET/POST /api/local/ai/catalog
GET      /api/local/ai/composer/candidates
POST     /api/local/ai/composer/rebind
GET/POST /api/local/ai/threads
GET/PUT  /api/device-workspaces
GET/PUT  /api/workflow-capabilities
```

---

## 7. Business Rules

- Rotas `/api/*` não encontradas retornam `404 NOT_FOUND`.
- Rotas estáticas fallback para `index.html`.
- Query params desconhecidos em rotas específicas retornam `400 UNKNOWN_QUERY_PARAMETER`.
- Métodos não permitidos retornam `405` com `Allow` header.

---

## 8. Domain Modeling

Ver SPEC-002.

---

## 9. Expected Architecture

ASP.NET Core Minimal APIs com `MapGroup` por recurso. Middleware de correlation id e global exception handler retornando ProblemDetails customizado.

---

## 10. API Contracts

### Exemplo: listar tarefas

```http
GET /api/tasks?projectId=local&status=todo&archived=false
```

Response:

```json
{
  "tasks": [ { "id": "...", "identifier": "TASK-local-1", ... } ],
  "project": { "id": "local", "name": "全局" }
}
```

### Exemplo: criar tarefa

```http
POST /api/tasks
{
  "projectId": "local",
  "title": "Implementar login",
  "description": "...",
  "status": "todo",
  "priority": "high",
  "labels": ["特性"]
}
```

### Error responses

| Status | Code | Quando |
|---|---|---|
| 400 | INVALID_PATH | path malformado |
| 400 | UNKNOWN_QUERY_PARAMETER | query inesperada |
| 404 | TASK_NOT_FOUND | tarefa inexistente |
| 404 | ATTACHMENT_NOT_FOUND | anexo inexistente |
| 409 | VERSION_CONFLICT | version obsoleto |
| 409 | PROJECT_EXISTS | projeto duplicado |
| 409 | JIRA_*_UNAVAILABLE | operações proibidas em Jira |
| 502 | JIRA_RECONCILE_FAILED | sync falhou |
| 500 | INTERNAL_ERROR | erro interno |

---

## 11. Application Contracts

```csharp
public sealed record ListTasksQuery(string? ProjectId, string? Status, bool? Archived, string? Q, string? AssigneeId, string? Label) : IRequest<TaskListDto>;
public sealed record GetTaskQuery(TaskId Id) : IRequest<TaskDto>;
public sealed record CreateTaskCommand(...) : IRequest<TaskDto>;
public sealed record UpdateTaskCommand(...) : IRequest<TaskDto>;
public sealed record MoveTaskCommand(...) : IRequest<TaskDto>;
public sealed record ArchiveTaskCommand(TaskId Id, long Version, string? ThreadId, ThreadBinding? ThreadBinding, Actor Actor) : IRequest<TaskDto>;
public sealed record RestoreTaskCommand(TaskId Id, long Version, string? ThreadId, ThreadBinding? ThreadBinding, Actor Actor) : IRequest<TaskDto>;
public sealed record DeleteTaskCommand(TaskId Id, long Version) : IRequest;
```

---

## 12. Persistence and Data

Ver SPEC-009.

---

## 13. Integrations

Ver SPEC-010 e SPEC-011.

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| ID muito longo | >128 chars | 400 INVALID_PATH |
| Query param não esperado | `?foo=bar` em GET /api/tasks/:id | 400 UNKNOWN_QUERY_PARAMETER |
| Versão errada | version=1 quando current=2 | 409 VERSION_CONFLICT |
| Tarefa Jira arquivada | source=jira + archive | 409 JIRA_ARCHIVE_UNAVAILABLE |

---

## 15. Few-Shot Examples

### Exemplo: PATCH tarefa

```http
PATCH /api/tasks/task-123
{
  "version": 3,
  "changes": { "title": "Novo título", "priority": "urgent" }
}
```

---

## 16-24. Standard SSD sections

---

## Pending Questions

1. Usar Minimal APIs ou controllers MVC?
2. Como implementar SSE em produção (IAsyncEnumerable, canal)?

## Human Approval Checklist

Seguir template padrão SSD.
