# SPEC-008: Frontend

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Frontend |
| Product / System | taskboard-ai |
| Module / Bounded Context | Presentation |
| Change type | Migration |
| Repository | afonsoft/taskboard-ai |
| Suggested branch | `devin/spec-webui-net10` |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O frontend atual usa React 19 + Vite + TypeScript com componentes para board, gantt, workflow, filtros, chat AI, comentários, anexos, etc. A decisão de curto prazo é servir a SPA React existente; a longo prazo reescrever em Blazor/.NET MAUI.

### Objective

- Fase 1: configurar `Taskboard.Server` para servir a SPA React/Vite buildada (`wwwroot/`), fallback para `index.html`.
- Fase 2 (futura): reescrever UI em Blazor Server / .NET MAUI Blazor Hybrid, consumindo REST API + SSE.

### Expected outcome

- Fase 1: build Vite servido estaticamente, rotas `/api` preservadas.
- Fase 2: componentes Blazor para board, cards, chat, workflow.

### Out of scope

- Reescrita para Blazor nesta fase (a menos que decidido).

---

## 2. Agent Role

> Frontend/ASP.NET engineer.

---

## 3. Agent Autonomy Level

3

### Restrictions

- Não modificar componentes React sem decisão explícita.
- Preservar endpoints `/api` e `/api/events`.

---

## 4. Product Context

### Functional context

A UI oferece board Kanban, filtros, gantt, editor de tarefas, chat AI, workflow visual, comentários e anexos.

### Technical context

- Vite build gera `dist/`.
- Componentes em `web/src/components/`.
- Bibliotecas: React 19, XYFlow, dhtmlx-gantt, react-markdown, mermaid.

### Relevant stack

- React 19 + Vite (fase 1)
- ASP.NET Core static files
- Blazor / .NET MAUI (fase futura)

---

## 5. Task Definition

### Main task

Configurar `Taskboard.Server` para servir a SPA React/Vite e planejar reescrita Blazor.

### Subtasks

- Servir `wwwroot/` estático.
- Fallback para `index.html`.
- Configurar CORS para dev.
- Preservar endpoints `/api` e `/api/events`.
- (Opcional) Estrutura Blazor/MAUI para reescrita.

### Do not do

- Não reescrever componentes React sem decisão.

---

## 6. Functional Requirements

### FR-001: Static files

**Description:**  
Servir build Vite (`dist/`) como `wwwroot` no host ASP.NET.

### FR-002: SPA fallback

**Description:**  
Todas as rotas não-API retornam `index.html`.

### FR-003: CORS

**Description:**  
Permitir origens de desenvolvimento (`http://localhost:5173`).

### FR-004: Board view (futuro Blazor)

- Colunas por status; cards por `sort_order`.
- Markdown GFM + mermaid read-only.
- HTML comments ocultos; raw HTML desabilitado.
- SSE realtime via `/api/events`; reconnect full refresh.
- AI chat via `/api/local/ai/threads/:id/events`.
- Storage local via `/api/client-storage`.

### FR-005: .NET MAUI desktop (futuro)

- Blazor Hybrid no WebView nativo (macOS/Windows).
- Equivale ao Tauri do original.

---

## 7. Business Rules

- Rotas `/api/**` nunca caem no fallback.
- Arquivos existentes em `wwwroot` são servidos antes do fallback.

---

## 8. Domain Modeling

Não aplica.

---

## 9. Expected Architecture

```text
# Fase 1
Taskboard.Server/wwwroot/
  index.html
  assets/

# Fase 2
src/Taskboard.Blazor/
  Components/
    BoardView.razor
    TaskCard.razor
    DashboardView.razor
    WorkflowNode.razor
    IssueMentionMenu.razor
    ProjectAutomationMenu.razor
    TaskContextMenu.razor
    InlineMediaComposer.razor
    PendingAttachments.razor
  Services/
    TaskboardClient.cs
    EventStream.cs
    AiChatState.cs
    RevisionPolling.cs
src/Taskboard.Maui/
  MainPage.xaml
  Platforms/
```

---

## 10. API Contracts

Ver `SPEC-002-rest-api.md`.

---

## 11. Application Contracts

Não aplica.

---

## 12. Persistence and Data

Não aplica.

---

## 13. Integrations

API REST (`SPEC-002`) e SSE.

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Build ausente | wwwroot vazio | mensagem de build não encontrado |
| Rota API | /api/tasks | não cai no fallback |
| CORS origin inválido | origin bloqueado | 403 |

---

## 15. Few-Shot Examples

```csharp
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
```

---

## 16. Non-Functional Requirements

- Startup < 1s.
- Bundle servido com cache-control apropriado.

---

## 17. Mandatory Guardrails

- Não expor `.env` ou config sensível no `wwwroot`.
- Preservar roteamento `/api`.

---

## 18. Expected Tests

| Flow | Validation |
|---|---|
| GET /index.html | 200 |
| GET /api/tasks | não cai no fallback |
| CORS preflight | 204 |

---

## 19. Acceptance Criteria

- [ ] Static files servidos.
- [ ] Fallback funciona.
- [ ] `/api` preservado.
- [ ] (Opcional) Estrutura Blazor preparada.

---

## 20. Implementation Plan

1. Configurar `wwwroot` no `Taskboard.Server`.
2. Adicionar `MapFallbackToFile`.
3. Configurar CORS.
4. (Opcional) Criar `Taskboard.Blazor` e `Taskboard.Maui`.

---

## 21. Rollback Strategy

- Reverter para servidor estático anterior.
- Restaurar build Vite.

---

## 22. Risks and Mitigations

| Risk | Impact | Probability | Mitigation |
|---|---|---:|---|
| Reescrita Blazor grande | Alto | Média | Fase 1 serve React; fase 2 iterativa |
| Paridade realtime/SSE | Médio | Média | Usar Blazor Server para SSE |

---

## 23. Definition of Done

- [ ] SPA servida ou estrutura Blazor criada.
- [ ] Tests de fallback.

---

## 24. Key Reminder

> The SPEC is the contract.

## Pending Questions

1. Manter React ou migrar para Blazor no futuro?
2. Empacotamento desktop com MAUI ou Avalonia?

## Human Approval Checklist

- [ ] Estratégia de UI decidida.
- [ ] Static files e fallback definidos.
