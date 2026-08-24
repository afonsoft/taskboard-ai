# SPEC-005: AI Chat

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | AI Chat |
| Product / System | taskboard-ai |
| Module / Bounded Context | AI / Workflow |
| Change type | Migration |
| Repository | afonsoft/taskboard-ai |
| Suggested branch | `devin/spec-ai-workflow-net10` |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O Taskboard possui chat com agentes Codex (`ai_chat_threads`, `ai_chat_runs`, `ai_chat_events`), catalog de modelos, composer candidates e rebind.

### Objective

Especificar módulo de IA chat em .NET 10: threads, runs, events, catalog e composer.

### Expected outcome

- `AiChatThread`, `AiChatRun`, `AiChatEvent`.
- Catalog de modelos e composer candidates.
- SSE por thread.

### Out of scope

- Integração real com modelos LLM (especificar contratos; implementação depende de provider).

---

## 2. Agent Role

> Senior AI/Backend engineer.

---

## 3. Agent Autonomy Level

3

### Restrictions

- Não implementar streaming real de LLM sem provider definido.
- Não expor API keys.

---

## 4. Product Context

### Functional context

Agentes de IA podem conversar, executar ações e registrar eventos.

### Technical context

- Tabelas `ai_chat_*` e `workflow_workspaces`.
- Eventos SSE por thread.
- Catalog `/api/local/ai/catalog`.

### Relevant stack

- .NET 10
- HttpClient para LLM providers
- SSE

---

## 5. Task Definition

### Main task

Mapear AI chat, catalog e composer.

### Subtasks

- CRUD de threads/runs/events.
- Catalog de modelos.
- Composer candidates/rebind.

### Do not do

- Não implementar streaming real de LLM sem provider definido.

---

## 6. Functional Requirements

### FR-001: AI Chat Threads

**Description:**  
Criar, listar, atualizar e deletar threads de IA.

### FR-002: AI Chat Runs

**Description:**  
Iniciar, finalizar runs com status e exit_code.

### FR-003: AI Chat Events

**Description:**  
Registrar eventos de `user`, `assistant`, `activity`, `error`.

### FR-004: AI Catalog

**Description:**  
Listar modelos disponíveis e `reasoning_effort`.

### FR-005: Composer

**Description:**  
Retornar candidates e rebind de thread.

### FR-006: SSE per thread

**Description:**  
`GET /api/local/ai/threads/:id/events` emite eventos por thread.

---

## 7. Business Rules

- Thread possui runs e events.
- Status permitidos: `idle`, `running`, `failed`.
- Sandbox: `read-only`, `workspace-write`, `danger-full-access`.
- Events imutáveis.

---

## 8. Domain Modeling

### Aggregates

| Aggregate | Responsibility | Invariants |
|---|---|---|
| AiChatThread | Gerencia runs e events | status válido |

### Entities

| Entity | Identity | Responsibility |
|---|---|---|
| AiChatThread | ThreadId | Conversa |
| AiChatRun | RunId | Execução |
| AiChatEvent | EventId | Evento |

### Value Objects

| Value Object | Fields | Validations |
|---|---|---|
| Sandbox | string | `read-only`, `workspace-write`, `danger-full-access` |
| ModelRef | string | in catalog |

---

## 9. Expected Architecture

`Taskboard.AiChat` module com serviços de abstração para LLM providers.

```text
src/Taskboard.AiChat/
  Domain/
  Application/
  Infrastructure/
```

---

## 10. API Contracts

```http
GET/POST /api/local/ai/threads
GET     /api/local/ai/threads/:id/events
GET/POST /api/local/ai/catalog
GET     /api/local/ai/composer/candidates
POST    /api/local/ai/composer/rebind
```

---

## 11. Application Contracts

```csharp
public sealed record CreateAiChatThreadCommand(
    string Title,
    string OriginProjectId,
    string Model,
    string ReasoningEffort,
    string Sandbox
) : IRequest<AiChatThreadDto>;

public sealed record AddAiChatEventCommand(
    ThreadId ThreadId,
    string Role,
    JsonElement Content
) : IRequest;
```

---

## 12. Persistence and Data

Ver `SPEC-011-persistence.md`.

---

## 13. Integrations

| Service | Data sent | Data received | Security |
|---|---|---|---|
| OpenAI/Claude API | prompts | completions | API key |

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Modelo inválido | catalog não contém | 400 |
| Thread não encontrada | GET /api/local/ai/threads/invalid | 404 |
| Run status inválido | `status` fora do enum | 400 |

---

## 15. Few-Shot Examples

```http
POST /api/local/ai/threads
{
  "title": "Refactor plan",
  "originProjectId": "local",
  "model": "gpt-4o",
  "reasoningEffort": "medium",
  "sandbox": "read-only"
}
```

---

## 16. Non-Functional Requirements

- Latência de catalog < 100ms.
- SSE por thread com reconnect.

---

## 17. Mandatory Guardrails

- Não logar API keys.
- Não persistir prompts sensíveis sem consentimento.

---

## 18. Expected Tests

| Flow | Validation |
|---|---|
| POST /api/local/ai/threads | 201 |
| GET /api/local/ai/catalog | retorna modelos |
| SSE thread events | event stream |

---

## 19. Acceptance Criteria

- [ ] Threads/runs/events mapeados.
- [ ] Catalog e composer funcional.
- [ ] SSE por thread.

---

## 20. Implementation Plan

1. Criar domain `AiChatThread`, `AiChatRun`, `AiChatEvent`.
2. Criar application commands/queries.
3. Mapear endpoints em `Taskboard.Server`.
4. Implementar SSE por thread.

---

## 21. Rollback Strategy

- Desabilitar feature flag de AI chat.
- Reverter endpoints.

---

## 22. Risks and Mitigations

| Risk | Impact | Probability | Mitigation |
|---|---|---:|---|
| Provider não definido | Alto | Alta | Deixar abstração; mock inicial |

---

## 23. Definition of Done

- [ ] SPEC revisado.
- [ ] Contratos claros.
- [ ] Abstração de provider.

---

## 24. Key Reminder

> The SPEC is the contract.

## Pending Questions

1. Qual provider de LLM será suportado (OpenAI, Anthropic, Azure OpenAI)?
2. O streaming de respostas é obrigatório?

## Human Approval Checklist

- [ ] Threads/runs/events definidos.
- [ ] Catalog e composer claros.
