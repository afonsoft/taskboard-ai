# SPEC-009: Persistência e Banco de Dados

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Persistência e Banco de Dados |
| Product / System | dashi-taskboard |
| Module / Bounded Context | Infrastructure |
| Change type | Migration |
| Repository | afonsoft/dashi-taskboard |
| Suggested branch | devin/spec-persistence-net10 |
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

- Sincronização D1/R2 (SPEC-010).

---

## 2. Agent Role

> Senior EF Core/SQLite engineer.

---

## 3. Agent Autonomy Level

3

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
Tabelas: Projects, Tasks, Comments, TaskActivities, Attachments, WorkflowWorkspaces, ProjectSummaries, AiChatThreads, AiChatRuns, AiChatEvents, TaskRelations.

### FR-002: Constraints

**Description:**  
Manter `CHECK` de enums, `UNIQUE`, `FOREIGN KEY`, `NOT NULL`.

### FR-003: Indexes

**Description:**  
Recriar índices para desempenho: `tasks_project_status_sort`, `comments_task_created`, `task_activities_task_created`, `attachments_task_created`, etc.

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

Ver SPEC-002.

---

## 9. Expected Architecture

`Taskboard.EntityFrameworkCore` com `DbContext`, `IRepository<T>` implementado e UnitOfWork via ABP.

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

### Migration required

Yes

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

Nenhuma.

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Concorrência na numeração | dois create task | transação serializável |
| SQLite locked | timeout | retry com exponential backoff |

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
    }
}
```

---

## 16-24. Standard SSD sections

---

## Pending Questions

1. Usar EF Core SQLite ou ABP `IRepository` padrão?
2. Migrations automáticas em produção? (local-first: sim)

## Human Approval Checklist

Seguir template padrão SSD.
