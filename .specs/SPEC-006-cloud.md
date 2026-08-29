# SPEC-006: Cloud

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Cloud Mode |
| Product / System | taskboard-ai |
| Module / Bounded Context | Cloud |
| Change type | Migration |
| Repository | afonsoft/taskboard-ai |
| Suggested branch | `devin/spec-cloud-net10` |
| Technical owner | afonsoft |
| Status | Implemented |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O Taskboard suporta modo cloud: companion loopback, proxy Cloudflare D1/R2, Basic Auth e polling de revisão.

### Objective

Especificar módulo cloud em .NET 10: companion device-local, Cloudflare proxy, sync e autenticação.

### Expected outcome

- `Taskboard.Cloud` com companion loopback, proxy D1/R2, Basic Auth.
- Endpoints `/api/local/cloud-session`, `/api/meta`.

### Out of scope

- Deploy real de Cloudflare Workers/D1/R2 (configuração only).

---

## 2. Agent Role

> Senior backend engineer com experiência em proxy, sync e autenticação.

---

## 3. Agent Autonomy Level

3

### Restrictions

- Não persistir credenciais em texto plano.
- Não exponer tokens de Cloudflare.

---

## 4. Product Context

### Functional context

Modo nuvem permite compartilhamento de boards entre devices e colaboração remota.

### Technical context

- `server/cloud-config.mjs`.
- Cloudflare D1 (banco) e R2 (anexos).
- Basic Auth no companion.
- Polling de revisão a cada 2s.

### Relevant stack

- .NET 10
- HttpClient
- Cloudflare API

---

## 5. Task Definition

### Main task

Mapear cloud companion, proxy e autenticação.

### Subtasks

- Companion loopback device-local.
- Proxy Cloudflare D1/R2.
- Basic Auth.
- Polling de revisão.

### Do not do

- Não implementar deploy automático.

---

## 6. Functional Requirements

### FR-001: Cloud Session

**Endpoints:**

```http
GET/PUT /api/local/cloud-session
```

**Regras:**

- GET retorna sessão ativa.
- PUT configura companion URL, credentials, projeto.

### FR-002: Meta

**Endpoint:** `GET /api/meta`  
**Response:** metadados do servidor incluindo `realtime:{transport:'poll',intervalMs:2000}`.

### FR-003: Companion Loopback

**Description:**  
Serviço local que espelha API para o companion cloud.

### FR-004: Proxy Cloudflare

**Description:**  
Proxy para endpoints D1/R2 autenticados.

### FR-005: Review Polling

**Description:**  
Polling a cada 2s para detectar revisões cloud.

---

## 7. Business Rules

- Credenciais armazenadas com permissão `0600` ou em secret store.
- Basic Auth no companion.
- Sync não sobrescreve local sem version conflict handling.

---

## 8. Domain Modeling

Nenhum novo; reutiliza `Project`, `Task`, `Attachment`.

---

## 9. Expected Architecture

```text
src/Taskboard.Cloud/
  Services/
    CloudCompanionService.cs
    CloudflareProxyService.cs
    CloudSyncService.cs
```

---

## 10. API Contracts

```http
GET/PUT /api/local/cloud-session
GET     /api/meta
```

---

## 11. Application Contracts

```csharp
public sealed record UpdateCloudSessionCommand(string? CompanionUrl, string? Username, string? Password, string? ProjectId) : IRequest;
public sealed record GetCloudSessionQuery() : IRequest<CloudSessionDto>;
```

---

## 12. Persistence and Data

Ver `SPEC-011-persistence.md`. Credenciais em arquivo local protegido.

---

## 13. Integrations

| Service | Data sent | Data received | Security |
|---|---|---|---|
| Cloudflare D1 | queries | rows | API token |
| Cloudflare R2 | files | files | API token |

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Companion offline | timeout | retry com backoff |
| Credenciais inválidas | 401 | 401 UNAUTHORIZED |
| Conflito de versão | sync com version stale | 409 |

---

## 15. Few-Shot Examples

```http
PUT /api/local/cloud-session
{
  "companionUrl": "https://companion.example.com",
  "username": "user",
  "password": "$CLOUD_PASSWORD",
  "projectId": "my-project"
}
```

---

## 16. Non-Functional Requirements

- Polling < 100ms overhead.
- Retry com exponential backoff.

---

## 17. Mandatory Guardrails

- Nunca logar senhas.
- Nunca commitar credenciais.

---

## 18. Expected Tests

| Flow | Validation |
|---|---|
| GET /api/meta | retorna realtime config |
| PUT /api/local/cloud-session | persiste config |
| Sync manual | pull/push de tasks |

---

## 19. Acceptance Criteria

- [ ] Cloud session endpoints.
- [ ] Polling configurável.
- [ ] Proxy D1/R2 especificado.

---

## 20. Implementation Plan

1. Criar `Taskboard.Cloud`.
2. Implementar `CloudSession` config.
3. Implementar proxy e sync.
4. Integrar endpoints em `Taskboard.Server`.

---

## 21. Rollback Strategy

- Desabilitar cloud mode.
- Restaurar backup local.

---

## 22. Risks and Mitigations

| Risk | Impact | Probability | Mitigation |
|---|---|---:|---|
| Latência Cloudflare | Médio | Média | Cache local, sync incremental |
| Segurança de credenciais | Alto | Média | Secret store, cifragem |

---

## 23. Definition of Done

- [ ] SPEC revisado.
- [ ] Contratos claros.
- [ ] Estratégia de segurança.

---

## 24. Key Reminder

> The SPEC is the contract.

## Pending Questions

1. Cloudflare é obrigatório ou opcional?
2. Companion deve rodar como serviço separado ou no mesmo processo?

## Human Approval Checklist

- [ ] Cloud session mapeado.
- [ ] Proxy e sync claros.
- [ ] Segurança de credenciais.
