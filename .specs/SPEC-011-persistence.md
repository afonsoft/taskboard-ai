# SPEC-011: Persistence

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Persistence and Database |
| Product / System | taskboard-ai |
| Module / Bounded Context | Infrastructure |
| Change type | Migration |
| Repository | afonsoft/taskboard-ai |
| Suggested branch | `devin/spec-persistence-net10` |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O sistema atual usa `node:sqlite` com schema evolutivo (ALTER TABLE, migrations manuais). É necessário mapear para EF Core + SQLite com Migrations.

### Objective

Criar `Taskboard.EntityFrameworkCore` com EF Core 10, SQLite, configuração de entidades, repositórios e migrations iniciais equivalentes ao schema final do Node.js.

### Expected outcome

- `DbContext` com `Projects`, `Tasks`, `Comments`, `Attachments`, `TaskActivities`, `TaskRelations`, `WorkflowWorkspaces`, `ProjectSummaries`, `AiChatThreads`, `AiChatRuns`, `AiChatEvents`.
- Repositórios implementando `IRepository<T>`.
- Migrations iniciais e seeds de dados (`local`).

### Out of scope

- Sincronização D1/R2 (ver `SPEC-006`, `SPEC-010`).

---

## 2. Agent Role

> Senior EF Core/SQLite engineer.

---

## 3. Agent Autonomy Level

3

### Restrictions

- Não colocar lógica de negócio nos repositórios.
- Não expor connection strings em appsettings.

---

## 4. Product Context

### Functional context

Banco local-first em `.data/taskboard.sqlite`. Pode ser copiado/backupeado.

### Technical context

- `DatabaseSync` do `node:sqlite`.
- Schema final com constraints, indexes e FKs.
- Timestamps ISO string.

### Relevant stack

- EF Core 10
- SQLite
- ABP N-Layer

---

## 5. Task Definition

### Main task

Mapear schema para EF Core e implementar repositórios.

### Subtasks

- Criar `TaskboardDbContext`.
- Configurar entidades (FluentAPI).
- Mapear constraints e indexes.
- Criar repositórios.
- Criar migration inicial.
- Seed projeto `local`.

### Do not do

- Não colocar lógica de negócio nos repositórios.

---

## 6. Functional Requirements

### FR-001: Entidades

**Description:**  
Tabelas: `Projects`, `Tasks`, `Comments`, `TaskActivities`, `Attachments`, `WorkflowWorkspaces`, `ProjectSummaries`, `AiChatThreads`, `AiChatRuns`, `AiChatEvents`, `TaskRelations`.

### FR-002: Constraints

**Description:**  
Manter `CHECK` de enums, `UNIQUE`, `FOREIGN KEY`, `NOT NULL`.

### FR-003: Indexes

**Description:**  
Recriar índices para desempenho.

### FR-004: Migration e Seed

**Description:**  
Migration inicial cria tabelas e insere projeto `local`.

---

## 7. Business Rules

- `Projects.Id = 'local'` não é deletável.
- Identificadores únicos.
- FKs com `ON DELETE CASCADE` apropriado.

---

## 8. Domain Modeling

Ver `SPEC-001-domain-model.md`.

---

## 9. Expected Architecture

`Taskboard.EntityFrameworkCore` com `DbContext`, `IRepository<T>` implementado e UnitOfWork via ABP.

```text
src/Taskboard.EntityFrameworkCore/
  Data/
    TaskboardDbContext.cs
  Configurations/
    ProjectConfiguration.cs
    TaskConfiguration.cs
    CommentConfiguration.cs
    AttachmentConfiguration.cs
    TaskRelationConfiguration.cs
    TaskActivityConfiguration.cs
    WorkflowWorkspaceConfiguration.cs
    AiChatThreadConfiguration.cs
    AiChatRunConfiguration.cs
    AiChatEventConfiguration.cs
    ProjectSummaryConfiguration.cs
  Repositories/
    EfCoreProjectRepository.cs
    EfCoreTaskRepository.cs
  Migrations/
```

---

## 10. API Contracts

Não aplica.

---

## 11. Application Contracts

Não aplica.

---

## 12. Persistence and Data

### Persisted entities

| Table | Purpose |
|---|---|
| Projects | Projetos |
| Tasks | Tarefas |
| Comments | Comentários |
| TaskActivities | Log de mudanças |
| Attachments | Anexos |
| WorkflowWorkspaces | Config JSON do board |
| ProjectSummaries | Resumos gerados |
| AiChatThreads | Threads de IA |
| AiChatRuns | Execuções de IA |
| AiChatEvents | Eventos de IA |
| TaskRelations | Relacionamentos |

### Schema highlights

#### `projects`

- `id` (text PK, <=128)
- `name` (text, not null)
- `workspace_path` (text, nullable)
- `labels` (text JSON)
- `next_task_number` (integer, default 1)
- `created_at`, `updated_at` (text ISO 8601)

#### `tasks`

- `id` (text PK, <=128)
- `identifier` (text, unique, not null)
- `project_id` (text FK projects.id)
- `title` (text <=240)
- `status` (text CHECK)
- `priority` (text CHECK)
- `labels` (text JSON)
- `sort_order` (real)
- `thread_id`, `thread_source`, `thread_name`, `thread_url`, `thread_references` (text)
- `creator_*`, `assignee_*` (text)
- `workflow_id` (text)
- `git_branch`, `worktree_path`, `worktree_branch` (text)
- `start_date`, `due_date` (text)
- `recurrence_interval`, `recurrence_unit` (text)
- `external_source`, `external_origin`, `external_id`, `external_key`, `external_url` (text)
- `archived_at` (text, nullable)
- `version` (integer, default 1)
- `created_at`, `updated_at` (text)

### Migration required

Yes.

### Migration strategy

- UP: criação de todas as tabelas e indexes.
- DOWN: drop tables na ordem inversa.

### Indexes

| Index | Fields | Reason |
|---|---|---|
| IX_Tasks_Project_Status_Sort | project_id, archived_at, status, sort_order, created_at | listagem board |
| IX_Comments_Task_Created | task_id, created_at, id | comentários por tarefa |
| IX_TaskActivities_Task_Created | task_id, created_at, id | atividades |
| IX_Attachments_Task_Created | task_id, created_at, id | anexos |
| UIX_Tasks_External | external_source, external_origin, external_id | unicidade externa |
| UIX_TaskRelations_Parent | target_task_id where relation_type='parent' | um pai |

### Compatibility

- [x] Não quebra dados existentes (migração limpa).
- [x] Inclui rollback.
- [x] Testes de migração.
- [x] Não expõe dados sensíveis.

---

## 13. Integrations

Nenhuma externa.

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Concorrência na numeração | dois create task | transação serializável |
| SQLite locked | timeout | retry com exponential backoff |
| Identifier duplicado | constraint | DbUpdateException |

---

## 15. Few-Shot Examples

```csharp
public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(t => new { t.ProjectId, t.ArchivedAt, t.Status, t.SortOrder, t.CreatedAt })
               .HasDatabaseName("IX_Tasks_Project_Status_Sort");
        builder.HasIndex(t => new { t.ExternalSource, t.ExternalOrigin, t.ExternalId })
               .IsUnique()
               .HasDatabaseName("UIX_Tasks_External");
    }
}
```

---

## 16. Non-Functional Requirements

- Migrations < 5s para criar schema.
- Queries de board otimizadas por indexes.

---

## 17. Mandatory Guardrails

- Não colocar regras de negócio nos repositórios.
- Connection string via env (`CODEX_TASKBOARD_DATA_DIR`) ou appsettings sem secrets.

---

## 18. Expected Tests

| Test | Validation |
|---|---|
| Migration runs | schema criado |
| Seed local | projeto local existe |
| Concurrency | numeração atômica |
| Repository CRUD | operações básicas |

---

## 19. Acceptance Criteria

- [ ] DbContext criado.
- [ ] Configurações de entidades.
- [ ] Indexes mapeados.
- [ ] Migration inicial.
- [ ] Seed `local`.

---

## 20. Implementation Plan

1. Criar `Taskboard.EntityFrameworkCore`.
2. Configurar `TaskboardDbContext`.
3. Configurar entidades (FluentAPI).
4. Criar repositórios EfCore.
5. Criar migration inicial.
6. Seed `local` project.
7. Testes de migration e repositório.

---

## 21. Rollback Strategy

- Reverter migration.
- Restaurar backup do SQLite.

---

## 22. Risks and Mitigations

| Risk | Impact | Probability | Mitigation |
|---|---|---:|---|
| Diferença de tipos SQLite vs EF Core | Médio | Média | Mapear explicitamente, testar |
| Migrations em produção local-first | Médio | Baixa | `EnsureCreated` ou `Migrate` controlado |

---

## 23. Definition of Done

- [ ] Schema mapeado.
- [ ] Migration funciona.
- [ ] Seed `local`.
- [ ] Tests passam.

---

## 24. Key Reminder

> The SPEC is the contract.

## Pending Questions

1. Usar EF Core SQLite ou ABP `IRepository` padrão?
2. Migrations automáticas em produção? (local-first: sim)

## Human Approval Checklist

- [ ] Schema completo.
- [ ] Indexes e constraints.
- [ ] Migration e seed.
