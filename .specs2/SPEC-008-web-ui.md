# SPEC-008: Web UI

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Web UI |
| Product / System | dashi-taskboard |
| Module / Bounded Context | Presentation |
| Change type | Migration |
| Repository | afonsoft/dashi-taskboard |
| Suggested branch | devin/spec-webui-net10 |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O frontend atual usa React 19 + Vite + TypeScript com componentes para board, gantt, workflow, filtros, chat AI, comentários, anexos, etc.

### Objective

Decidir estratégia de UI: manter React/Vite servido estaticamente pelo ASP.NET Core, ou reimplementar em Blazor/MAUI em fase futura.

### Expected outcome

Nesta fase, servir a SPA React/Vite buildada pelo `Taskboard.HttpApi.Host`. Reaproveitar os componentes TypeScript/React existentes sem modificação.

### Out of scope

- Reescrita para Blazor nesta fase.

---

## 2. Agent Role

> Frontend/ASP.NET engineer.

---

## 3. Agent Autonomy Level

3

---

## 4. Product Context

### Functional context

A UI oferece board Kanban, filtros, gantt, editor de tarefas, chat AI, workflow visual, comentários e anexos.

### Technical context

- Vite build gera `dist/`.
- Componentes em `web/src/components/`.
- Bibliotecas: React 19, XYFlow, dhtmlx-gantt, react-markdown, mermaid.

### Relevant stack

- React 19
- Vite
- ASP.NET Core static files
- (opcional futuro) Blazor/MAUI

---

## 5. Task Definition

### Main task

Configurar `Taskboard.HttpApi.Host` para servir a SPA React/Vite e reescrever proxy dev se necessário.

### Subtasks

- Servir `wwwroot/` estático.
- Fallback para `index.html`.
- Configurar CORS para dev.
- Preservar endpoints `/api` e `/api/events`.

### Do not do

- Não reescrever componentes React.

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

---

## 7. Business Rules

- Rotas `/api/**` nunca caem no fallback.
- Arquivos existentes em wwwroot são servidos antes do fallback.

---

## 8. Domain Modeling

Não aplica.

---

## 9. Expected Architecture

```text
Taskboard.HttpApi.Host
  wwwroot/
    index.html
    assets/
  MapStaticAssets()
  MapFallbackToFile("index.html")
```

---

## 10-11. API/Application Contracts

Não aplica.

---

## 12. Persistence and Data

Não aplica.

---

## 13. Integrations

API REST (SPEC-004) e SSE.

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Build ausente | wwwroot vazio | mensagem de build não encontrado |
| Rota API | /api/tasks | não cai no fallback |

---

## 15. Few-Shot Examples

```csharp
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
```

---

## 16-24. Standard SSD sections

---

## Pending Questions

1. Manter React ou migrar para Blazor no futuro?
2. Empacotamento desktop com MAUI ou Avalonia?

## Human Approval Checklist

Seguir template padrão SSD.
