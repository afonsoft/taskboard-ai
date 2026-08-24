# Spec (legado/referência): Frontend React original

> Referência do app React/Vite do sistema-fonte. O clone **não** o reutiliza —
> veja `SPEC-008-frontend.md` (reescrita em Blazor/.NET MAUI). Mantido aqui
> para mapear comportamento 1:1.

## Stack
- React 18 + TypeScript, **Vite** (`vite.config.ts`, `tsconfig.json`).
- `web/src/`: `App.tsx`, `main.tsx`, `api.ts`, `types.ts`, `storage.ts`,
  `i18n.tsx`, `actors.ts`, `aiChatState.ts`, `taskConversations.ts`,
  `taskFilters.ts`, `issueBoardStatuses.ts`, `issueRoute.ts`, `labels.ts`,
  `revisionPolling.mjs`, `embeddedHost.mjs`, `workflowStore.ts`, `styles.css`.
- 38 componentes em `web/src/components/` (TaskCard, TaskboardIcon, DashboardView,
  WorkflowNode, WorkflowMark, IssueMentionMenu, ProjectAutomationMenu,
  TaskContextMenu, BoardCardDisplayMenu, InlineMediaComposer, PendingAttachments…).
- `web/public/`: fontes (Inter), favicon, logos. `index.html` entry.

## Comportamento
- **Board view**: colunas por status; cards por `sort_order`; markdown GFM +
  mermaid read-only; HTML comments ocultos; raw HTML desabilitado.
- **SSE realtime**: `revisionPolling.mjs` + `/api/events` → atualiza board;
  reconnect faz full refresh.
- **AI chat**: `aiChatState.ts` consome `/api/local/ai/threads/:id/events`.
- **Embedded host**: `embeddedHost.mjs` injeta painel no Codex (sidebar + iframe).
- **i18n**: `i18n.tsx` + `workflowI18n.ts`.
- **Storage local**: `storage.ts` → `/api/client-storage`.

## Build
```bash
npm run build:web   # Vite → sirvo estático pelo backend
npm run dev         # :5173, proxy :47823
```
Desktop (Tauri): `npm run app:dev` / `npm run app:build` (macOS universal, Windows NSIS).
