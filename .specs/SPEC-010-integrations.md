# SPEC-010: Integrations

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Integrations |
| Product / System | taskboard-ai |
| Module / Bounded Context | Integrations |
| Change type | Migration |
| Repository | afonsoft/taskboard-ai |
| Suggested branch | `devin/spec-integrations-net10` |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O sistema suporta sincronização com Jira e integração DeepSeek, além de módulos compartilhados de execução de processos (`shared/*.mjs`).

### Objective

Especificar integrações externas no .NET 10: Jira sync, DeepSeek harness e infraestrutura de execução de processos.

### Expected outcome

- `IJiraService` e `ICloudProxyService`.
- `Taskboard.Integrations` project.
- Serviços de execução de processos (`CodexExecutableResolver`, `ProcessTreeSignaler`, `ExecutableCommand`).

### Out of scope

- Implementação completa de UI de configuração Jira (ver `SPEC-008`).

---

## 2. Agent Role

> Senior integration engineer (Jira REST API, Cloudflare, OAuth, process management).

---

## 3. Agent Autonomy Level

3

### Restrictions

- Não logar tokens.
- Não commitar credenciais.

---

## 4. Product Context

### Functional context

Permitir que o Taskboard sincronize tarefas com Jira e opção de modo cloud compartilhado.

### Technical context

- Jira project ID fixo `jira-my-tasks`.
- `server/cloud-config.mjs` e handlers de proxy.
- Cloudflare D1/R2.

### Relevant stack

- .NET 10
- HttpClient
- OAuth / Basic Auth
- Cloudflare API

---

## 5. Task Definition

### Main task

Mapear integrações Jira, DeepSeek e infra de execução para .NET 10.

### Subtasks

- Configuração de conexão Jira.
- Sync de issues (pull e push).
- Proxy cloud (ver `SPEC-006`).
- Armazenamento de anexos em R2.
- Módulos de execução compartilhados.

### Do not do

- Não implementar UI nesta spec.

---

## 6. Functional Requirements

### FR-001: Jira Connection

**Description:**  
Configurar URL, email, token e projeto Jira. Testar conexão.

**Endpoints:**

```http
GET  /api/local/jira-connection
POST /api/local/jira-connection
POST /api/local/jira-connection/sync
```

### FR-002: Jira Sync

**Description:**  
- Pull: trazer issues do Jira para o projeto `jira-my-tasks`.
- Push: atualizar Jira quando tarefas locais forem alteradas (se source=jira).

### FR-003: DeepSeek Harness

**Description:**  
Plugin/adaptador para ecossistema DeepSeek usar o mesmo `taskctl` CLI.

### FR-004: Shared execution modules

**Description:**  
- `CodexExecutableResolver` — resolve path do executável Codex.
- `ProcessTreeSignaler` — mata árvore de processos.
- `WithoutTaskboardEnv` — remove `CODEX_TASKBOARD_*` do env do child.
- `ExecutableCommand` — wrapper para executar `.cjs/.js/.mjs` via node.

---

## 7. Business Rules

- Tarefas Jira não podem ser arquivadas/deletadas pelo Taskboard.
- Conflito de versão retorna 409.
- Sync pode arquivar tarefas ausentes no Jira.
- Labels JIRA protegidas contra delete (409 em `POST/DELETE /api/projects/:id/labels`).
- Credenciais Jira/cloud: nunca logar nem commitar; modo `0600` no arquivo local.

---

## 8. Domain Modeling

Ver `SPEC-001-domain-model.md`.

---

## 9. Expected Architecture

`Taskboard.Integrations` project com `IJiraService`, `ICloudProxyService`, `ICloudStorageService`, `IExecutableResolver`, `IProcessTreeSignaler`.

```text
src/Taskboard.Integrations/
  Jira/
    IJiraService.cs
    JiraIntegration.cs
  Cloud/
    ICloudProxyService.cs
    ICloudStorageService.cs
  Execution/
    CodexExecutableResolver.cs
    ProcessTreeSignaler.cs
    ExecutableCommand.cs
    WithoutTaskboardEnv.cs
```

---

## 10. API Contracts

```http
GET  /api/local/jira-connection
POST /api/local/jira-connection
POST /api/local/jira-connection/sync
```

---

## 11. Application Contracts

```csharp
public sealed record SyncJiraCommand() : IRequest<SyncResultDto>;
public sealed record UpdateJiraTaskCommand(TaskId Id, TaskPatch Changes) : IRequest;
```

---

## 12. Persistence and Data

Ver `SPEC-011-persistence.md`. Credenciais em secret store/arquivo protegido.

---

## 13. Integrations

| Service | Data sent | Data received | Security |
|---|---|---|---|
| Jira REST API | updates | issues, comments | Basic/OAuth token |
| Cloudflare D1 | queries | rows | API token |
| Cloudflare R2 | files | files | API token |

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Jira offline | timeout | 502 |
| Token inválido | 401 do Jira | 401/403 |
| Conflito Jira | version mismatch | 409 |
| Label JIRA protegida | delete label JIRA | 409 |

---

## 15. Few-Shot Examples

```bash
POST /api/local/jira-connection
{ "url": "https://x.atlassian.net", "email": "a@b.com", "token": "$JIRA_TOKEN" }
```

---

## 16. Non-Functional Requirements

- Sync Jira < 30s para boards medianos.
- Circuit breaker para Jira.

---

## 17. Mandatory Guardrails

- Não expor tokens.
- Não commitar credenciais.
- Usar secret store.

---

## 18. Expected Tests

| Flow | Validation |
|---|---|
| POST /api/local/jira-connection | 200 |
| Sync pull | cria/ atualiza tasks source=jira |
| Label JIRA delete | 409 |

---

## 19. Acceptance Criteria

- [ ] Jira connection endpoints.
- [ ] Sync especificado.
- [ ] DeepSeek harness adapter.
- [ ] Execution modules mapeados.

---

## 20. Implementation Plan

1. Criar `Taskboard.Integrations`.
2. Implementar Jira service.
3. Implementar cloud proxy/storage.
4. Implementar execution helpers.
5. Integrar endpoints.

---

## 21. Rollback Strategy

- Desabilitar Jira sync.
- Restaurar backup local.

---

## 22. Risks and Mitigations

| Risk | Impact | Probability | Mitigation |
|---|---|---:|---|
| API Jira instável | Médio | Média | Circuit breaker, retry |
| Credenciais vazadas | Alto | Baixa | Secret store |

---

## 23. Definition of Done

- [ ] Integrações mapeadas.
- [ ] Segurança documentada.

---

## 24. Key Reminder

> The SPEC is the contract.

## Pending Questions

1. Suportar apenas Jira Cloud ou também Jira Server/Data Center?
2. Cloudflare obrigatório ou opcional?
3. DeepSeek harness é adapter no mesmo repo ou pacote separado?

## Human Approval Checklist

- [ ] Jira sync claro.
- [ ] Segurança de credenciais.
