# SPEC-002: REST API

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | REST API e Servidor HTTP |
| Product / System | taskboard-ai |
| Module / Bounded Context | Taskboard HTTP API |
| Change type | Migration |
| Repository | afonsoft/taskboard-ai |
| Suggested branch | `devin/spec-httpapi-net10` |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O servidor atual `server/app.mjs` (3.300+ linhas) usa `node:http` raw e roteia manualmente para `health`, storage local, projetos, tarefas, eventos SSE, anexos, comentários, workflow, AI e cloud/Jira.

### Objective

Mapear todos os endpoints HTTP do Node.js para ASP.NET Core Minimal APIs / controllers, preservando rotas, métodos, query params, payloads e códigos de erro.

### Expected outcome

Aplicação `Taskboard.Server` expondo todos os endpoints com ProblemDetails e error codes customizados.

### Out of scope

- Implementação do client frontend (ver `SPEC-008`).
- Deploy Cloudflare (ver `SPEC-006`, `SPEC-010`).

---

## 2. Agent Role

> Senior ASP.NET Core engineer com experiência em Minimal APIs, SSE, file streaming e ProblemDetails.

### Expected behavior

- Preserve exact route contracts.
- Map errors to ProblemDetails with custom `code` fields.
- Implement SSE with correct `text/event-stream` format.
- Support instance-token auth and CORS.

---

## 3. Agent Autonomy Level

### Selected level

3

### Restrictions

- Não alterar rotas HTTP sem documentar breaking change.
- Não introduzir autenticação complexa além do token de instância.

---

## 4. Product Context

### Functional context

Servidor local-first escutando em `0.0.0.0`/`127.0.0.1` na porta `CODEX_TASKBOARD_PORT` (default `47823`). Serve a SPA estática e expõe API JSON.

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
- `Microsoft.Data.Sqlite`

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

- Não reimplementar lógica de IA sem `SPEC-005`.
- Não reimplementar Jira sem `SPEC-010`.

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
- POST cria com `id`, `name`, `workspacePath`.
- Projeto `local` sempre inicializado.

### FR-003: Tarefas

**Endpoints:**

```http
GET    /api/tasks?projectId=&status=&archived=&...
POST   /api/tasks
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

- Query params para listagem: `projectId`, `status`, `q`, `assigneeId`, `label`, `archived`, etc.
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
- Header `Authorization` contém instance token para endpoints sensíveis.

---

## 8. Domain Modeling

Ver `SPEC-001-domain-model.md`.

---

## 9. Expected Architecture

ASP.NET Core Minimal APIs com `MapGroup` por recurso. Middleware de correlation id e global exception handler retornando ProblemDetails customizado.

### Middleware

- `CorrelationIdMiddleware`
- `GlobalExceptionHandlerMiddleware` → ProblemDetails
- `InstanceTokenAuthMiddleware`
- `CorsMiddleware`

### SSE implementation

```csharp
app.MapGet("/api/events", async (HttpResponse response, IEventStreamService events, CancellationToken ct) =>
{
    response.Headers.ContentType = "text/event-stream";
    await foreach (var ev in events.SubscribeAsync(ct))
    {
        await response.WriteAsync($"event: {ev.Type}\n");
        await response.WriteAsync($"data: {JsonSerializer.Serialize(ev.Payload)}\n\n");
        await response.Body.FlushAsync(ct);
    }
});
```

---

## 10. API Contracts

### Listar tarefas

```http
GET /api/tasks?projectId=local&status=todo&archived=false
```

Response:

```json
{
  "tasks": [ { "id": "...", "identifier": "TASK-local-1" } ],
  "project": { "id": "local", "name": "全局" }
}
```

### Criar tarefa

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

### PATCH tarefa

```http
PATCH /api/tasks/task-123
{
  "version": 3,
  "changes": { "title": "Novo título", "priority": "urgent" }
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
public sealed record ListTasksQuery(
    string? ProjectId,
    string? Status,
    bool? Archived,
    string? Q,
    string? AssigneeId,
    string? Label
) : IRequest<TaskListDto>;

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

Ver `SPEC-011-persistence.md`.

---

## 13. Integrations

Ver `SPEC-006-cloud.md`, `SPEC-010-integrations.md`.

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| ID muito longo | >128 chars | 400 INVALID_PATH |
| Query param não esperado | `?foo=bar` em GET /api/tasks/:id | 400 UNKNOWN_QUERY_PARAMETER |
| Versão errada | version=1 quando current=2 | 409 VERSION_CONFLICT |
| Tarefa Jira arquivada | source=jira + archive | 409 JIRA_ARCHIVE_UNAVAILABLE |
| Servidor offline | — | health falha |

---

## 15. Few-Shot Examples

### Exemplo: criação de projeto

```http
POST /api/projects
{
  "id": "my-project",
  "name": "My Project",
  "workspacePath": "/home/user/my-project"
}
```

Response:

```json
{
  "project": {
    "id": "my-project",
    "name": "My Project",
    "workspacePath": "/home/user/my-project",
    "labels": ["缺陷", "特性", "for-claude", "hold", "改进", "phase-1", "phase-2", "phase-3", "phase-4", "phase-5", "phase-6"],
    "issueCount": 0,
    "createdAt": "2026-08-24T01:53:00Z",
    "updatedAt": "2026-08-24T01:53:00Z"
  }
}
```

---

## 16. Non-Functional Requirements

- P95 < 300ms para operações de leitura.
- SSE reconnect faz full refresh no cliente.
- CORS configurado para `http://localhost:5173` em dev.
- Static files e fallback SPA funcionam.

---

## 17. Mandatory Guardrails

- Não alterar contratos sem versionamento/documentação.
- Não colocar regras de negócio em endpoints.
- Não expor dados sensíveis em ProblemDetails.
- Respeitar cancellation tokens.

---

## 18. Expected Tests

| Flow | Validation |
|---|---|
| GET /health | 200 |
| POST /api/projects | 201 Created |
| GET /api/projects | retorna lista com local |
| POST /api/tasks | cria com identifier correto |
| PATCH /api/tasks/:id | version conflict 409 |
| GET /api/events | SSE stream funciona |
| Static files | fallback para index.html |

---

## 19. Acceptance Criteria

- [ ] Todos os endpoints mapeados.
- [ ] ProblemDetails customizado.
- [ ] SSE funcional.
- [ ] CORS e auth por token.
- [ ] Static files fallback.

---

## 20. Implementation Plan

1. Criar `Taskboard.Server` project.
2. Configurar Minimal APIs e middleware.
3. Mapear health, meta, client-storage.
4. Mapear `/api/projects`.
5. Mapear `/api/tasks` e sub-recursos.
6. Mapear `/api/attachments`.
7. Mapear `/api/events` SSE.
8. Mapear local endpoints (AI, cloud, Jira, workflow).
9. Configurar static files e SPA fallback.
10. Escrever integration tests com `WebApplicationFactory`.

---

## 21. Rollback Strategy

- Reverter branch.
- Restaurar backup do SQLite.
- Desabilitar feature flag.

---

## 22. Risks and Mitigations

| Risk | Impact | Probability | Mitigation |
|---|---|---:|---|
| Diferença de roteamento raw vs Minimal APIs | Médio | Média | Testar rotas 1:1 |
| SSE scaling | Médio | Baixa | In-memory channel para MVP |

---

## 23. Definition of Done

- [ ] SPEC revisado.
- [ ] Todos os endpoints mapeados.
- [ ] Tests automatizados.
- [ ] Build validado.

---

## 24. Key Reminder

> The SPEC is the contract.

## Pending Questions

1. Usar Minimal APIs ou controllers MVC?
2. Como implementar SSE em produção (`IAsyncEnumerable`, canal, SignalR)?
3. Static files: servir build Vite existente ou gerar novo?

## Human Approval Checklist

- [ ] Contratos API explícitos.
- [ ] Códigos de erro mapeados.
- [ ] SSE e CORS considerados.
- [ ] Static files e fallback definidos.
