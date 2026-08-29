# SPEC-012 (legacy): Frontend React original

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Frontend React original (reference) |
| Product / System | taskboard-ai |
| Module / Bounded Context | Presentation |
| Change type | Reference |
| Repository | afonsoft/taskboard-ai |
| Suggested branch | — |
| Technical owner | afonsoft |
| Status | Implemented |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

> Referência do app React/Vite do sistema-fonte. O clone **não** o reutiliza nesta fase — veja `SPEC-008-frontend.md`. Mantido aqui para mapear comportamento 1:1.

### Stack

- React 18 + TypeScript, **Vite** (`vite.config.ts`, `tsconfig.json`).
- `web/src/`: `App.tsx`, `main.tsx`, `api.ts`, `types.ts`, `storage.ts`, `i18n.tsx`, `actors.ts`, `aiChatState.ts`, `taskConversations.ts`, `taskFilters.ts`, `issueBoardStatuses.ts`, `issueRoute.ts`, `labels.ts`, `revisionPolling.mjs`, `embeddedHost.mjs`, `workflowStore.ts`, `styles.css`.
- 38 componentes em `web/src/components/`.
- `web/public/`: fontes (Inter), favicon, logos. `index.html` entry.

### Comportamento

- **Board view**: colunas por status; cards por `sort_order`; markdown GFM + mermaid read-only; HTML comments ocultos; raw HTML desabilitado.
- **SSE realtime**: `revisionPolling.mjs` + `/api/events` → atualiza board; reconnect faz full refresh.
- **AI chat**: `aiChatState.ts` consome `/api/local/ai/threads/:id/events`.
- **Embedded host**: `embeddedHost.mjs` injeta painel no Codex (sidebar + iframe).
- **i18n**: `i18n.tsx` + `workflowI18n.ts`.
- **Storage local**: `storage.ts` → `/api/client-storage`.

### Build

```bash
npm run build:web   # Vite → sirvo estático pelo backend
npm run dev         # :5173, proxy :47823
```

Desktop (Tauri): `npm run app:dev` / `npm run app:build` (macOS universal, Windows NSIS).

---

## 2. Agent Role

> Reference only; do not implement unless `SPEC-008-frontend.md` decides to keep React.

---

## 3. Agent Autonomy Level

0 (reference document).

---

## 24. Key Reminder

> Preserve this file for behavior mapping. Do not delete.
