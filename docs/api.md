# API Documentation

## REST Endpoints

### Projects

```http
GET    /api/projects
POST   /api/projects
GET    /api/projects/:id
PUT    /api/projects/:id
POST   /api/projects/:id/archive
DELETE /api/projects/:id
```

### Tasks

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

### Comments & Attachments

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

### Context & Storage

```http
GET    /api/context
PUT    /api/context
GET    /api/client-storage
PUT    /api/client-storage
```

### AI

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

### Search

```http
POST   /api/search/semantic
GET    /api/search/suggestions
```

## SSE

### Global events

```http
GET /api/events
Accept: text/event-stream
```

Events: `task.created`, `task.updated`, `task.archived`, `comment.added`, `comment.deleted`, `attachment.added`, `attachment.deleted`.

### Per-thread AI events

```http
GET /api/local/ai/threads/:id/events
Accept: text/event-stream
```

## Error Contract

All errors use RFC 7807 `ProblemDetails` with optional `ErrorCode`:

```json
{
  "type": "https://taskboard/errors/validation",
  "title": "Validation failed",
  "status": 400,
  "detail": "Status must be one of: todo, in_progress, in_review, done, blocked, canceled",
  "ErrorCode": "VALIDATION_ERROR"
}
```

Special code `VERSION_CONFLICT` with HTTP 409 for optimistic concurrency failures.

## JSON Envelope

Responses are plain JSON. The CLI `taskctl` wraps API responses in:

```json
{
  "ok": true,
  "data": { ... },
  "error": null
}
```

See `.specs/SPEC-002-rest-api.md` and `.specs/SPEC-003-cli.md` for full contracts.
