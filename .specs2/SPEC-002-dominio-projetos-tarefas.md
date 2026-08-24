# SPEC-002: Domínio - Projetos, Tarefas, Status e Ciclo de Vida

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Domínio de Projetos e Tarefas |
| Product / System | dashi-taskboard |
| Module / Bounded Context | Taskboard Core |
| Change type | Migration / Design |
| Repository | afonsoft/dashi-taskboard |
| Suggested branch | devin/spec-domain-net10 |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O domínio atual em `shared/domain.mjs` e `server/database.mjs` define statuses, prioridades, projetos, tarefas, labels e identificadores. A migração para .NET 10 deve reproduzir esse domínio com tipos fortes, validações e eventos de domínio.

### Objective

Projetar o modelo de domínio C# para `Project`, `Task`, `Comment`, `Attachment`, `TaskRelation` e `WorkflowWorkspace`, refletindo fielmente o schema e regras do Node.js.

### Expected outcome

Classes de domínio imutáveis/seguras, rich models, value objects para `TaskStatus`, `TaskPriority`, `Actor`, `Recurrence` e `TaskIdentifier`.

### Out of scope

- Implementação de controllers/handlers nesta spec (ver SPEC-004).
- Persistência EF Core (ver SPEC-009).

---

## 2. Agent Role

> You are a senior C# domain modeler using DDD, .NET 10, ABP N-Layer.

---

## 3. Agent Autonomy Level

3

### Restrictions

- Não alterar contratos HTTP.
- Não introduzir tipos sem justificativa.

---

## 4. Product Context

### Functional context

O Taskboard agrupa tarefas em projetos. Cada projeto mantém contador sequencial `next_task_number`, labels próprias e path do workspace. Tarefas transitam pelos status: backlog, todo, in_progress, in_review, blocked, done, canceled.

### Technical context

Atualmente os estados são constantes JS em `shared/domain.mjs`. O `database.mjs` usa SQLite `CHECK` constraints.

### Relevant stack

- .NET 10, C# 14
- ABP N-Layer Domain module
- xUnit + Shouldly + NSubstitute

---

## 5. Task Definition

### Main task

Criar o modelo de domínio C# equivalente ao domínio Node.js.

### Subtasks

- Value objects: TaskStatus, TaskPriority, Actor, Recurrence, TaskIdentifier, ProjectId, TaskId.
- Aggregates: Project, Task.
- Entities: Comment, Attachment.
- Relacionamentos: TaskRelation.
- Domain events para mutações de tarefa.

### Do not do

- Não criar DTOs nesta camada.
- Não referenciar EF Core no Domain.

---

## 6. Functional Requirements

### FR-001: Status e prioridades tipadas

**Description:**  
O domínio deve restringir `TaskStatus` aos valores: backlog, todo, in_progress, in_review, blocked, done, canceled.  
E `TaskPriority` a: none, urgent, high, medium, low.

**Rules:**

- Status e prioridades são case-sensitive strings.
- Valores inválidos lançam `DomainException`.

**Inputs:**

| Field | Type | Required | Rule |
|---|---|---:|---|
| value | string | yes | dentro do enum permitido |

**Outputs:**

| Field | Type | Description |
|---|---|---|
| status/priority | value object | validado |

**Acceptance criteria:**

- [ ] Testes Dado/Quando/Então para todos os valores válidos e inválidos.

---

### FR-002: Ciclo de vida da tarefa

**Description:**  
Uma tarefa pode ser criada, movida entre statuses, atualizada (PATCH), arquivada, restaurada e deletada permanentemente (apenas se arquivada e não Jira).

**Rules:**

- Tarefas Jira não podem ser arquivadas/restauradas/deletadas pelo Taskboard.
- Versão incrementa a cada mutação.
- `archived_at` null indica ativa.

**Acceptance criteria:**

- [ ] `Task.Move(status, sortOrder)` atualiza status e ordem.
- [ ] `Task.Archive()` seta `ArchivedAt`.
- [ ] `Task.Restore()` limpa `ArchivedAt`.

---

### FR-003: Identificadores e numeração

**Description:**  
Projeto gera tarefas com identifier `TASK-{projectId}-{number}`. Jira usa `JIRA:{origin}:{externalKey}`.

**Rules:**

- `next_task_number` incrementa de forma atômica.
- Identificadores são únicos globalmente.

**Acceptance criteria:**

- [ ] `Project.GenerateTaskIdentifier()` retorna string correta.

---

## 7. Business Rules

### BR-001: Status fixos

Apenas os sete status definidos são aceitos.

### BR-002: Prioridades fixas

Apenas as cinco prioridades definidas são aceitas.

### BR-003: Versão otimista

Cada mutação incrementa `version`. PATCH requer `version` e falha em conflito.

### BR-004: Tarefas arquivadas imutáveis

Tarefas arquivadas não podem ser atualizadas nem movidas. Devem ser restauradas primeiro.

### BR-005: Projeto local global

Existe sempre o projeto `local` com nome `全局` e `workspacePath` null.

### Domain invariants

- Task `project_id` não muda.
- `TaskRelation` não permite auto-relacionamento nem `related` duplicado invertido.
- Cada tarefa só pode ter um pai (`parent`).

---

## 8. Domain Modeling

### Bounded Context

Taskboard Core

### Aggregates

| Aggregate | Responsibility | Invariants |
|---|---|---|
| Project | Criação, labels, numeração | next_task_number crescente |
| Task | Ciclo de vida, atributos, versionamento | status válido, version > 0 |

### Entities

| Entity | Identity | Responsibility |
|---|---|---|
| Comment | CommentId | Registro textual anexado a uma Task |
| Attachment | AttachmentId | Arquivo anexado a Task/Comment |
| TaskRelation | composto | Ligação entre duas Tasks |
| WorkflowWorkspace | ProjectId | Config JSON de board visual |

### Value Objects

| Value Object | Fields | Validations |
|---|---|---|
| TaskStatus | string Value | in TASK_STATUSES |
| TaskPriority | string Value | in TASK_PRIORITIES |
| Actor | Type, Id, Name, AvatarUrl | Type in ('user','agent') |
| Recurrence | Interval, Unit | Unit in ('day','week','month','year') |
| TaskIdentifier | string Value | formato correto, único |
| ProjectId | string Value | <=128 chars |
| TaskId | string Value | <=128 chars |

### Domain Events

| Event | When it occurs | Payload |
|---|---|---|
| TaskCreatedDomainEvent | Após criação | TaskId, ProjectId |
| TaskMovedDomainEvent | Após move | TaskId, OldStatus, NewStatus |
| TaskUpdatedDomainEvent | Após patch | TaskId, ChangedFields |
| TaskArchivedDomainEvent | Após archive | TaskId |
| TaskRestoredDomainEvent | Após restore | TaskId |
| ProjectLabelsUpdatedDomainEvent | Após novo label | ProjectId |

### Expected C# style

```csharp
public sealed class Task : AggregateRoot<TaskId>
{
    private TaskStatus _status;
    public TaskStatus Status => _status;
    public TaskPriority Priority { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public long Version { get; private set; } = 1;
    public DateTime? ArchivedAt { get; private set; }
    public ProjectId ProjectId { get; private set; }
    public TaskIdentifier Identifier { get; private set; }
    public IReadOnlyCollection<string> Labels => _labels.AsReadOnly();

    public void Move(TaskStatus newStatus, double? sortOrder, Actor actor)
    {
        if (ArchivedAt.HasValue) throw new DomainException("Tarefa arquivada não pode ser movida.");
        var old = _status;
        _status = newStatus;
        if (sortOrder.HasValue) SortOrder = sortOrder.Value;
        Version++;
        AddDomainEvent(new TaskMovedDomainEvent(Id, old.Value, newStatus.Value));
    }
}
```

---

## 9. Expected Architecture

ABP Domain layer: `Taskboard.Domain` project.

---

## 10. API Contracts

Ver SPEC-004 para endpoints.

---

## 11. Application Contracts

```csharp
public sealed record CreateTaskCommand(
    ProjectId ProjectId,
    string Title,
    string? Description,
    string Status,
    string Priority,
    IReadOnlyList<string> Labels,
    string? ThreadId,
    ThreadBinding? ThreadBinding,
    Actor Creator
) : IRequest<TaskDto>;
```

---

## 12. Persistence and Data

Ver SPEC-009.

---

## 13. Integrations

Nenhuma no domínio.

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Status inválido | "invalid" | DomainException |
| Mover tarefa arquivada | task.ArchivedAt != null | DomainException |
| Versão conflito | version != current | VersionConflictException |

---

## 15. Few-Shot Examples

### Exemplo: criação de tarefa

```csharp
var project = Project.Create(ProjectId.From("my-project"), "My Project");
var task = Task.Create(
    TaskId.NewGuid(),
    project.NextTaskIdentifier(),
    project.Id,
    "Nova feature",
    TaskStatus.Todo,
    TaskPriority.High,
    new[] { "特性" },
    Actor.LocalUser()
);
task.Identifier.Value.ShouldBe("TASK-my-project-1");
```

---

## 16. Non-Functional Requirements

- Value objects imutáveis.
- Domain events testáveis.

---

## 17. Mandatory Guardrails

- Domain não acessa infraestrutura.
- Sem `DateTime.Now` direto; usar `IClock` do ABP.

---

## 18. Expected Tests

| Class | Scenarios |
|---|---|
| TaskStatus | valores válidos/inválidos |
| TaskPriority | valores válidos/inválidos |
| Project | numeração, labels |
| Task | criação, move, archive, restore, versionamento |
| TaskRelation | parent único, related simétrico |

---

## 19. Acceptance Criteria

- [ ] Todos os value objects testados.
- [ ] Regras de tarefas Jira testadas.
- [ ] Eventos de domínio publicados corretamente.

---

## 20. Implementation Plan

1. Criar value objects.
2. Criar aggregates Project e Task.
3. Criar entities Comment, Attachment, TaskRelation, WorkflowWorkspace.
4. Criar domain events.
5. Escrever domain tests Dado/Quando/Então.

---

## 21. Rollback Strategy

- Reverter para modelos anteriores se quebrar contrato.

---

## 22. Risks and Mitigations

| Risk | Impact | Probability | Mitigation |
|---|---|---:|---|
| Diferença de semântica entre JS dinâmico e C# tipado | Médio | Média | Mapear tipos explicitamente |

---

## 23. Definition of Done

- [ ] Domain model testado e aprovado.
- [ ] SPEC revisado.

---

## 24. Key Reminder

The SPEC is the contract.

---

## Pending Questions

1. Os identificadores devem permanecer como string ou usar UUIDs internamente?
2. `sort_order` usa double ou decimal para evitar imprecisão?

## Human Approval Checklist

Seguir checklist padrão SSD.
