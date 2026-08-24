# SPEC-010: Integrações Jira e Cloud

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Integrações Jira e Cloud |
| Product / System | dashi-taskboard |
| Module / Bounded Context | Integrations |
| Change type | Migration |
| Repository | afonsoft/dashi-taskboard |
| Suggested branch | devin/spec-integrations-net10 |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O sistema suporta sincronização com Jira e modo cloud via Cloudflare (D1/R2). O `server/app.mjs` contém handlers de Jira e cloud proxy.

### Objective

Especificar integração Jira e cloud no .NET 10: autenticação, sync de issues, proxy cloud e storage remoto.

### Expected outcome

- `IJiraService` e `ICloudProxyService`.
- Configuração por `appsettings.json` e secrets.
- Sincronização bidirecional de issues.

### Out of scope

- Implementação completa de UI de configuração Jira (ver SPEC-008).

---

## 2. Agent Role

> Senior integration engineer (Jira REST API, Cloudflare, OAuth).

---

## 3. Agent Autonomy Level

3

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

Mapear integrações Jira e Cloud para .NET 10.

### Subtasks

- Configuração de conexão Jira.
- Sync de issues (pull e push).
- Proxy cloud.
- Armazenamento de anexos em R2.

### Do not do

- Não implementar UI nesta spec.

---

## 6. Functional Requirements

### FR-001: Jira Connection

**Description:**  
Configurar URL, email, token e projeto Jira. Testar conexão.

### FR-002: Jira Sync

**Description:**  
- Pull: trazer issues do Jira para o projeto `jira-my-tasks`.
- Push: atualizar Jira quando tarefas locais forem alteradas (se source=jira).

### FR-003: Cloud Proxy

**Description:**  
Proxy para endpoints cloud autenticados.

### FR-004: Cloud Storage

**Description:**  
Upload/download de anexos via R2.

---

## 7. Business Rules

- Tarefas Jira não podem ser arquivadas/deletadas pelo Taskboard.
- Conflito de versão retorna 409.
- Sync pode arquivar tarefas ausentes no Jira.

---

## 8. Domain Modeling

Ver SPEC-002.

---

## 9. Expected Architecture

`Taskboard.Integrations` project com `IJiraService`, `ICloudProxyService`, `ICloudStorageService`.

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

Ver SPEC-009.

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

---

## 15. Few-Shot Examples

```bash
POST /api/local/jira-connection
{ "url": "https://x.atlassian.net", "email": "a@b.com", "token": "$JIRA_TOKEN" }
```

---

## 16-24. Standard SSD sections

---

## Pending Questions

1. Suportar apenas Jira Cloud ou também Jira Server/Data Center?
2. Cloudflare obrigatório ou opcional?

## Human Approval Checklist

Seguir template padrão SSD.
