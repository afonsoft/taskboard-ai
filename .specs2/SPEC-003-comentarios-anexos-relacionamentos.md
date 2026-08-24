# SPEC-003: Comentários, Anexos, Relacionamentos e Atividades

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Comentários, Anexos, Relacionamentos e Atividades |
| Product / System | dashi-taskboard |
| Module / Bounded Context | Taskboard Collaboration |
| Change type | Migration / Design |
| Repository | afonsoft/dashi-taskboard |
| Suggested branch | devin/spec-collaboration-net10 |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O Taskboard permite comentários em tarefas, anexos vinculados a tarefas/comentários, relacionamentos entre tarefas (parent/blocks/related) e registro de atividades (`task_activities`).

### Objective

Especificar o domínio e aplicação para comentários, anexos, relacionamentos e atividades em .NET 10.

### Expected outcome

- `Comment` pode ser criado/atualizado/deletado.
- `Attachment` pode ser criado (upload), lido (content/download) e deletado.
- `TaskRelation` garante invariants de parent único e simetria de `related`.
- `TaskActivity` registra mudanças com `actor`, `changes` JSON e timestamp.

### Out of scope

- Sincronização de anexos para cloud (SPEC-010).
- Renderização de markdown (SPEC-008).

---

## 2. Agent Role

> Senior C# engineer focado em DDD, persistência e mediação de anexos.

---

## 3. Agent Autonomy Level

3

---

## 4. Product Context

### Functional context

Colaboração em tarefas: comentários, anexos (inline/attachment), relacionamentos hierárquicos e feed de atividades.

### Technical context

Schema SQLite define `comments`, `attachments`, `task_relations`, `task_activities`.

### Relevant stack

- .NET 10
- EF Core SQLite
- File storage local

---

## 5. Task Definition

### Main task

Modelar e implementar comentários, anexos, relacionamentos e atividades.

### Subtasks

- CRUD comentários.
- Upload/download/delete anexos.
- CRUD relacionamentos.
- Geração automática de atividades nas mudanças de tarefa.

### Do not do

- Não implementar preview de imagem nesta spec.
- Não sincronizar com Jira.

---

## 6. Functional Requirements

### FR-001: Comentários

**Description:**  
Criar, listar, atualizar e deletar comentários de uma tarefa.

**Rules:**

- Comentário pertence a uma `Task`.
- Author é um `Actor`.
- Suporta `thread_id` para ligação com agente Codex.
- Body não pode ser vazio após trim.

**Inputs:**

| Field | Type | Required | Rule |
|---|---|---:|---|
| taskId | string | yes | existente |
| body | string | yes | não vazio |
| author | Actor | yes | user/agent |
| threadId | string? | no | identificador de thread |

**Outputs:**

| Field | Type | Description |
|---|---|---|
| comment | CommentDto | comentário criado/atualizado |

**Acceptance criteria:**

- [ ] POST /api/tasks/:id/comments cria comentário.
- [ ] PATCH /api/tasks/:taskId/comments/:commentId atualiza body.
- [ ] DELETE remove comentário e anexos vinculados (cascata).

---

### FR-002: Anexos

**Description:**  
Upload de arquivos para tarefa ou comentário; download por `id/content` ou `id/download`; deleção.

**Rules:**

- `kind` = 'inline' ou 'attachment'.
- `size` >= 0.
- Arquivos armazenados em diretório configurável (`CODEX_TASKBOARD_ATTACHMENTS_DIR` / `.data/attachments`).
- Content-disposition: inline para `content` se `INLINE_ATTACHMENT_TYPES` contiver contentType; attachment caso contrário.
- Arquivos inexistentes retornam 404.

**Inputs:**

| Field | Type | Required | Rule |
|---|---|---:|---|
| taskId | string | yes | existente |
| commentId | string? | no | existente |
| file | IFormFile | yes | size >= 0 |

**Outputs:**

| Field | Type | Description |
|---|---|---|
| attachment | AttachmentDto | metadados do anexo |

**Acceptance criteria:**

- [ ] POST /api/attachments recebe multipart e retorna metadados.
- [ ] GET /api/attachments/:id/content retorna stream com content-type correto.
- [ ] DELETE /api/attachments/:id remove arquivo e registro.

---

### FR-003: Relacionamentos

**Description:**  
Criar/remover relacionamentos entre tarefas: parent, blocks, related.

**Rules:**

- source_task_id != target_task_id.
- `related` exige source < target lexicograficamente.
- Apenas um `parent` por tarefa filha (target).
- Deleção cascata quando tarefa é removida.

**Acceptance criteria:**

- [ ] POST /api/tasks/:id/relations cria relação.
- [ ] DELETE /api/tasks/:id/relations/:relationType/:targetTaskId remove.
- [ ] Tentativa de segundo pai retorna 409.

---

### FR-004: Atividades

**Description:**  
Registro imutável de mudanças em tarefas com actor e JSON de changes.

**Rules:**

- Criado automaticamente em create/update/move/archive/restore/delete.
- `changes` é JSON com campos alterados.

**Acceptance criteria:**

- [ ] Atividade gerada para cada mutação.
- [ ] Listagem por taskId ordenada por created_at.

---

## 7. Business Rules

### BR-001: Anexos pertencem a tarefas

Attachment sempre vinculado a uma Task; opcionalmente a um Comment.

### BR-002: Tamanho não negativo

`size` deve ser >= 0.

### BR-003: Parent único

Uma tarefa target só pode ter um parent.

### BR-004: Related simétrico

`related` é não-direcional; source < target garante unicidade.

---

## 8. Domain Modeling

### Aggregates

| Aggregate | Responsibility | Invariants |
|---|---|---|
| Task | Contém comentários e atividades | via FK |
| Attachment | Metadados de arquivo | size >= 0 |
| TaskRelation | Ligação entre tasks | regras de unicidade |

### Entities

| Entity | Identity | Responsibility |
|---|---|---|
| Comment | CommentId | Texto e metadados |
| Attachment | AttachmentId | Metadados de arquivo |
| TaskRelation | composto | Relacionamento |
| TaskActivity | TaskActivityId | Log de mudanças |

### Value Objects

| Value Object | Fields | Validations |
|---|---|---|
| AttachmentKind | string | 'inline' ou 'attachment' |
| RelationType | string | 'parent','blocks','related' |

---

## 9. Expected Architecture

ABP Application/Domain. Infrastructure implementa `IFileStorage`.

---

## 10. API Contracts

Ver SPEC-004.

---

## 11. Application Contracts

```csharp
public sealed record AddCommentCommand(TaskId TaskId, string Body, Actor Author, ThreadBinding? Thread) : IRequest<CommentDto>;
public sealed record UploadAttachmentCommand(TaskId TaskId, CommentId? CommentId, Stream Content, string FileName, string ContentType, long Size) : IRequest<AttachmentDto>;
public sealed record CreateRelationCommand(TaskId SourceTaskId, TaskId TargetTaskId, RelationType Type) : IRequest;
```

---

## 12. Persistence and Data

Ver SPEC-009.

---

## 13. Integrations

Nenhuma externa.

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Comentário vazio | "" | 400 |
| Anexo tamanho negativo | size=-1 | 400 |
| Relacionamento consigo | source==target | 400 |
| Segundo parent | parent já existe para target | 409 |

---

## 15. Few-Shot Examples

### Exemplo: upload de anexo

```csharp
await mediator.Send(new UploadAttachmentCommand(
    TaskId.From("task-1"),
    null,
    stream,
    "diagrama.png",
    "image/png",
    stream.Length
));
```

---

## 16-24. Standard SSD sections (guardrails, tests, acceptance, plan, rollback, risks, DoD)

Seguir template padrão.

---

## Pending Questions

1. Diretório de anexos deve ser configurável por env ou appsettings?
2. Limites de tamanho de arquivo?

## Human Approval Checklist

Seguir template padrão SSD.
