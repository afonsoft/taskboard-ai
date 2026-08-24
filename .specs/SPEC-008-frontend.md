# Spec: Frontend React (module `frontend`)

Descreve a UI do `dashi-taskboard` (`web/`, React + Vite + TypeScript). Servida
estaticamente pelo backend (`resolved.staticDirectory`); consome a REST API
(`SPEC-002`) e SSE (`/api/events`, `/api/local/ai/threads/:id/events`).

## Stack
- React 18 + TypeScript, **Vite** (`vite.config.ts`, `tsconfig.json`).
- UI em `web/src/`: `App.tsx`, `main.tsx`, `api.ts`, `types.ts`, `storage.ts`,
  `i18n.tsx`, `actors.ts`, `aiChatState.ts`, `taskConversations.ts`,
  `taskFilters.ts`, `issueBoardStatuses.ts`, `issueRoute.ts`, `labels.ts`,
  `revisionPolling.mjs`, `embeddedHost.mjs`, `workflowStore.ts`, `styles.css`.
- 38 componentes em `web/src/components/` (TaskCard, TaskboardIcon,
  DashboardView, WorkflowNode, WorkflowMark, IssueMentionMenu, ProjectAutomationMenu,
  TaskContextMenu, BoardCardDisplayMenu, InlineMediaComposer, PendingAttachments,
  etc.).
- `web/public/`: fontes (Inter), favicon, logos. `index.html` entry.

## Comportamento chave
- **Board view**: colunas por status (`issueBoardStatuses.ts`); cards ordenados
  por `sort_order`; markdown GFM + mermaid read-only em descrições/comentários;
  HTML comments ocultos; raw HTML desabilitado.
- **SSE realtime**: `revisionPolling.mjs` + `/api/events` → atualiza board;
  reconnect faz full refresh (não perde mudanças offline).
- **AI chat**: `aiChatState.ts` consome `/api/local/ai/threads/:id/events` (SSE
  por thread); composer com candidates (`/api/local/ai/composer/candidates`).
- **Embedded host**: `embeddedHost.mjs` injeta o painel no Codex (sidebar +
  iframe OOPIF); usa `window.__CODEX_TASKBOARD_URL__`, route bridge nativo.
- **i18n**: `i18n.tsx` (labels/strings); `workflowI18n.ts` para workflow.
- **Storage local**: `storage.ts` → `/api/client-storage` (GET/PATCH merge).

## Build & dev
```bash
npm run build:web          # produção (Vite) → sirvo estático pelo backend
npm run dev                # Vite em :5173, proxy p/ :47823
```
Build de produção do app desktop (Tauri): `npm run app:dev` / `npm run app:build`
(macOS universal, Windows NSIS).

## .NET mapping
- **Opção A (recomendada p/ MVP)**: reutilizar o app React existente, apontando
  `CODEX_TASKBOARD_URL` para o serviço .NET. Contrato HTTP/SSE idêntico garante
  funcionamento sem reescrita. Serve `web/dist` como static files no ASP.NET Core.
- **Opção B**: reimplementar em outro framework .NET (Blazor/MudBlazor). Fora do
  escopo inicial — requer nova spec de componentes.
- O clone deve servir os assets estáticos e expor `/api/*` no mesmo host/porta
  (paridade de origem para evitar CORS/issues de embed).
