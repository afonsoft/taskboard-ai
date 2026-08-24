# SPEC-011: IA, Chat Threads e Workflow Workspaces

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | IA, Chat Threads e Workflow Workspaces |
| Product / System | dashi-taskboard |
| Module / Bounded Context | AI / Workflow |
| Change type | Migration |
| Repository | afonsoft/dashi-taskboard |
| Suggested branch | devin/spec-ai-workflow-net10 |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O Taskboard possui chat com agentes Codex (`ai_chat_threads`, `ai_chat_runs`, `ai_chat_events`), catalog de modelos, composer candidates e workflow workspaces (JSON de board visual).

### Objective

Especificar módulo de IA e workflow workspaces em .NET 10.

### Expected outcome

- `AiChatThread`, `AiChatRun`, `AiChatEvent`.
- Catalog de modelos e composer.
- Workflow workspace JSON persistido por projeto.

### Out of scope

- Integração real com modelos LLM (especificar contratos; implementação depende de provider).

---

## 2. Agent Role

> Senior AI/Backend engineer.

---

## 3. Agent Autonomy Level

3

---

## 4. Product Context

### Functional context

Agentes de IA podem conversar, executar ações e registrar eventos. O workflow workspace permite customizar colunas e estados visuais.

### Technical context

- Tabelas `ai_chat_*` e `workflow_workspaces`.
- Eventos SSE.
- Catalog `/api/local/ai/catalog`.

### Relevant stack

- .NET 10
- HttpClient para LLM providers
- SSE

---

## 5. Task Definition

### Main task

Mapear AI chat, catalog e workflow workspaces.

### Subtasks

- CRUD de threads/runs/events.
- Catalog de modelos.
- Composer candidates/rebind.
- Workflow workspace GET/PUT.

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
Registrar eventos de user/assistant/activity/error.

### FR-004: AI Catalog

**Description:**  
Listar modelos disponíveis e reasoning_effort.

### FR-005: Composer

**Description:**  
Retornar candidates e rebind de thread.

### FR-006: Workflow Workspace

**Description:**  
GET/PUT workspace JSON por projeto.

---

## 7. Business Rules

- Thread possui runs e events.
- Status permitidos: idle, running, failed.
- Sandbox: read-only, workspace-write, danger-full-access.

---

## 8. Domain Modeling

### Aggregates

| Aggregate | Responsibility | Invariants |
|---|---|---|
| AiChatThread | Gerencia runs e events | status válido |
| WorkflowWorkspace | Config JSON por projeto | JSON válido |

### Entities

| Entity | Identity | Responsibility |
|---|---|---|
| AiChatThread | ThreadId | Conversa |
| AiChatRun | RunId | Execução |
| AiChatEvent | EventId | Evento |

---

## 9. Expected Architecture

`Taskboard.Ai` module com serviços de abstração para LLM providers.

---

## 10. API Contracts

```http
GET/POST /api/local/ai/threads
GET/POST /api/local/ai/catalog
GET     /api/local/ai/composer/candidates
POST    /api/local/ai/composer/rebind
GET/PUT /api/workflow-capabilities
GET/PUT /api/device-workspaces
```

---

## 11. Application Contracts

```csharp
public sealed record CreateAiChatThreadCommand(string Title, string OriginProjectId, string Model, string ReasoningEffort, string Sandbox) : IRequest<AiChatThreadDto>;
public sealed record UpdateWorkflowWorkspaceCommand(ProjectId ProjectId, JsonElement Workspace) : IRequest;
```

---

## 12. Persistence and Data

Ver SPEC-009.

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
| Workspace JSON inválido | syntax error | 400 |

---

## 15. Few-Shot Examples

```http
POST /api/local/ai/threads
{ "title": "Refactor plan", "originProjectId": "local", "model": "gpt-4o", "reasoningEffort": "medium", "sandbox": "read-only" }
```

---

## 16-24. Standard SSD sections

---

## Pending Questions

1. Qual provider de LLM será suportado (OpenAI, Anthropic, Azure OpenAI)?
2. O streaming de respostas é obrigatório?

## Human Approval Checklist

Seguir template padrão SSD.
