# Documentação da API

## Endpoints REST

### Projetos

```http
GET    /api/projects
POST   /api/projects
GET    /api/projects/:id
PUT    /api/projects/:id
POST   /api/projects/:id/archive
DELETE /api/projects/:id
```

### Tarefas

```http
GET    /api/projects/:id/tasks
POST   /api/projects/:id/tasks
GET    /api/tasks/:id
PUT    /api/tasks/:id
POST   /api/tasks/:id/archive
DELETE /api/tasks/:id
POST   /api/tasks/:id/comments
POST   /api/tasks/:id/attachments
POST   /api/tasks/:id/move
POST   /api/tasks/:id/relations
```

### Comentários e Anexos

```http
GET    /api/comments/:id
PUT    /api/comments/:id
DELETE /api/comments/:id
GET    /api/attachments/:id
PUT    /api/attachments/:id
DELETE /api/attachments/:id
```

### Labels

```http
GET    /api/projects/:id/labels
POST   /api/projects/:id/labels
DELETE /api/projects/:id/labels/:label
```

### Contexto e Storage

```http
GET    /api/context
PUT    /api/context
GET    /api/client-storage
PUT    /api/client-storage
```

### IA

```http
GET    /api/local/ai/threads
POST   /api/local/ai/threads
GET    /api/local/ai/threads/:id/events
GET    /api/local/ai/catalog
GET    /api/local/ai/composer/candidates
POST   /api/local/ai/composer/rebind
```

### Cloud

```http
GET    /api/meta
GET    /api/local/cloud-session
PUT    /api/local/cloud-session
```

### Workflow

```http
GET    /api/workflow-capabilities
PUT    /api/workflow-capabilities
GET    /api/device-workspaces
PUT    /api/device-workspaces
```

### Jira

```http
GET    /api/local/jira-connection
POST   /api/local/jira-connection
POST   /api/local/jira-connection/sync
```

### Busca

```http
POST   /api/search/semantic
GET    /api/search/suggestions
```

## SSE

### Eventos globais

```http
GET /api/events
Accept: text/event-stream
```

Eventos: `task.created`, `task.updated`, `task.archived`, `comment.added`, `comment.deleted`, `attachment.added`, `attachment.deleted`.

### Eventos por thread de IA

```http
GET /api/local/ai/threads/:id/events
Accept: text/event-stream
```

## Contrato de Erros

Todos os erros usam RFC 7807 `ProblemDetails` com `ErrorCode` opcional:

```json
{
  "type": "https://taskboard/errors/validation",
  "title": "Validation failed",
  "status": 400,
  "detail": "Status deve ser um dos: todo, in_progress, in_review, done, blocked, canceled",
  "ErrorCode": "VALIDATION_ERROR"
}
```

Código especial `VERSION_CONFLICT` com HTTP 409 para falhas de concorrência otimista.

## Envelope JSON

As respostas são JSON puro. O CLI `taskctl` envelopa as respostas da API:

```json
{
  "ok": true,
  "data": { ... },
  "error": null
}
```

Veja `.specs/SPEC-002-rest-api.md` e `.specs/SPEC-003-cli.md` para contratos completos.
