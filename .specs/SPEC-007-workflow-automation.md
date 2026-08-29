# SPEC-007: Workflow Automation

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Workflow Automation |
| Product / System | taskboard-ai |
| Module / Bounded Context | Workflow |
| Change type | Migration |
| Repository | afonsoft/taskboard-ai |
| Suggested branch | `devin/spec-workflow-net10` |
| Technical owner | afonsoft |
| Status | Implemented |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O Taskboard possui workflow workspaces (JSON de board visual), engine de grafo de workflow e auto-claim de tarefas `todo` por agentes Codex.

### Objective

Especificar módulo de workflow e automação em .NET 10.

### Expected outcome

- `WorkflowWorkspace` persistido por projeto.
- Control-flow engine para automação.
- Auto-claim de `todo` → `in_progress` com handoff de `threadBinding`.

### Out of scope

- UI visual do workflow (ver `SPEC-008`).

---

## 2. Agent Role

> Senior backend engineer com experiência em grafos, automação e agendamento.

---

## 3. Agent Autonomy Level

3

### Restrictions

- Não executar ações destrutivas sem confirmação.
- Não logar credenciais de Codex.

---

## 4. Product Context

### Functional context

Workflow permite customizar colunas, estados e automações por projeto. Auto-claim permite que agentes assumam tarefas `todo` automaticamente.

### Technical context

- Tabelas `workflow_workspaces`.
- `workflow-control-flow.mjs` / `workflow-sequence.mjs`.
- `taskboard-automation*.mjs`.

### Relevant stack

- .NET 10
- JSON schema validation
- Cron/scheduler (Hangfire ou HostedService)

---

## 5. Task Definition

### Main task

Mapear workflow workspaces e automação.

### Subtasks

- GET/PUT `/api/workflow-capabilities`.
- GET/PUT `/api/device-workspaces`.
- Control-flow engine.
- Auto-claim policy.

### Do not do

- Não implementar UI nesta spec.

---

## 6. Functional Requirements

### FR-001: Workflow Workspace

**Description:**  
GET/PUT JSON de configuração de board visual por projeto.

### FR-002: Workflow Capabilities

**Description:**  
GET/PUT capabilities de workflow por device/projeto.

### FR-003: Control-flow Engine

**Description:**  
Interpretar nós de workflow e executar ações condicionais.

### FR-004: Auto-claim

**Description:**  
Cron que claim `todo` para sessões Codex remotas via SSH, com handoff de `threadBinding`.

---

## 7. Business Rules

- Apenas tarefas `todo` podem ser auto-claimed.
- `threadBinding` deve ser respeitado.
- Conflito de versão → retry uma vez.

---

## 8. Domain Modeling

### Aggregates

| Aggregate | Responsibility | Invariants |
|---|---|---|
| WorkflowWorkspace | Config JSON por projeto | JSON válido |

### Entities

| Entity | Identity | Responsibility |
|---|---|---|
| WorkflowNode | NodeId | Nó do grafo |
| WorkflowSequence | SequenceId | Sequência de execução |

---

## 9. Expected Architecture

```text
src/Taskboard.Workflow/
  Domain/
    WorkflowWorkspace.cs
  Application/
    ControlFlowEngine.cs
  Infrastructure/
    WorkflowScheduler.cs
```

---

## 10. API Contracts

```http
GET/PUT /api/workflow-capabilities
GET/PUT /api/device-workspaces
```

---

## 11. Application Contracts

```csharp
public sealed record UpdateWorkflowWorkspaceCommand(ProjectId ProjectId, JsonElement Workspace) : IRequest;
public sealed record UpdateWorkflowCapabilitiesCommand(string DeviceId, JsonElement Capabilities) : IRequest;
```

---

## 12. Persistence and Data

Ver `SPEC-011-persistence.md`.

---

## 13. Integrations

| Service | Data sent | Data received | Security |
|---|---|---|---|
| Codex app-server | spawn requests | status | local socket |

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Workspace JSON inválido | syntax error | 400 |
| Auto-claim sem tarefas | nenhum todo | idle |
| Conflito de versão | retry uma vez | 409 se persistir |

---

## 15. Few-Shot Examples

```http
PUT /api/workflow-capabilities
{
  "deviceId": "dev-1",
  "capabilities": { "nodes": ["start", "claim", "review"] }
}
```

---

## 16. Non-Functional Requirements

- Scheduler precisão de 1 minuto.
- JSON schema validado previamente.

---

## 17. Mandatory Guardrails

- Não executar ações fora do escopo definido.
- Não expor `threadBinding` em logs.

---

## 18. Expected Tests

| Flow | Validation |
|---|---|
| PUT workspace | persistência |
| Auto-claim | move todo → in_progress |
| Capabilities | GET/PUT |

---

## 19. Acceptance Criteria

- [ ] Workflow workspace mapeado.
- [ ] Auto-claim especificado.
- [ ] Capabilities endpoints.

---

## 20. Implementation Plan

1. Criar `WorkflowWorkspace` aggregate.
2. Implementar GET/PUT endpoints.
3. Implementar control-flow engine.
4. Implementar auto-claim scheduler.

---

## 21. Rollback Strategy

- Desabilitar scheduler.
- Restaurar workspace anterior.

---

## 22. Risks and Mitigations

| Risk | Impact | Probability | Mitigation |
|---|---|---:|---|
| Complexidade de grafos | Médio | Média | Iniciar com sequences simples |
| Auto-claim incorreto | Alto | Média | Regras claras + logs |

---

## 23. Definition of Done

- [ ] SPEC revisado.
- [ ] Contratos claros.

---

## 24. Key Reminder

> The SPEC is the contract.

## Pending Questions

1. Usar Hangfire ou `IHostedService` para cron?
2. Auto-claim requer SSH ou spawn local?

## Human Approval Checklist

- [ ] Workflow workspace claro.
- [ ] Auto-claim com regras.
