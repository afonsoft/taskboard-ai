# Spec: Frontend Blazor / .NET MAUI (module `frontend`)

Descreve a **reescrita** da UI do `dashi-taskboard` (original React/Vite, ver
`SPEC-008-legacy-react.md`) em **Blazor** com alvo desktop via **.NET MAUI**
(ou Blazor Hybrid). Substitui o app React no clone; consome a mesma REST API
(`SPEC-002`) e SSE.

> Decisão confirmada (usuário): frontend **reescrito**, não reutilizado.

## Stack alvo
- **Blazor** (Server ou WebAssembly; recomendado **Server** p/ paridade de
  realtime/SSE e simplicidade de deploy no mesmo host do backend).
- **.NET MAUI** (Blazor Hybrid) para o app desktop (macOS/Windows), empacotando
  a UI Blazor num WebView nativo — equivalendo ao Tauri do original.
- **.NET MAUI UI Toolkit / .NET MAUI Controls** ou componentes tipo
  `MudBlazor`/`Radzen` para cards/colunas/diálogos.
- Linguagem: **C# 13 / Razor**. Compartilha `Taskboard.Domain` e `Taskboard.Client`.

## Comportamento a reproduzir (do original, `SPEC-008-legacy-react.md`)
- **Board view**: colunas por status (`issueBoardStatuses`); cards ordenados por
  `sort_order`; markdown GFM + mermaid read-only; HTML comments ocultos; raw
  HTML desabilitado.
- **SSE realtime**: consumir `/api/events`; reconnect → full refresh (não perder
  mudanças offline). Cloud: poll de revisão a cada 2s (`/api/meta` →
  `realtime:{transport:'poll',intervalMs:2000}`).
- **AI chat**: consumir `/api/local/ai/threads/:id/events` (SSE por thread);
  composer com candidates (`/api/local/ai/composer/candidates`).
- **i18n**: strings localizadas (labels/strings) + workflow i18n.
- **Storage local**: `/api/client-storage` (GET/PATCH merge).
- **Markdown rendering**: parser GFM (ex.: `Markdig`) + renderer mermaid
  (JS interop) ou componente read-only.

## Estrutura (alvo)
```
src/Taskboard.Blazor/            → App Blazor (Server)
  Components/
    BoardView.razor              → colunas por status
    TaskCard.razor               → card (markdown, ações)
    DashboardView.razor
    WorkflowNode.razor / WorkflowMark.razor
    IssueMentionMenu.razor
    ProjectAutomationMenu.razor
    TaskContextMenu.razor
    InlineMediaComposer.razor
    PendingAttachments.razor
  Services/
    TaskboardClient.cs           → wrapper da API (reuso de Taskboard.Client)
    EventStream.cs               → SSE /api/events + reconnect/full-refresh
    AiChatState.cs               → SSE por thread + composer candidates
    RevisionPolling.cs           → poll cloud
  wwwroot/                       → assets, fontes, favicon
src/Taskboard.Maui/              → Blazor Hybrid (desktop macOS/Windows)
  MainPage.xaml (+ BlazorWebView) → equipara Tauri app
  Platforms/                     → macOS (MacCatalyst), Windows (WinUI)
```

## Build & run
```bash
dotnet run --project src/Taskboard.Blazor        # UI em http://localhost:5000
dotnet build src/Taskboard.Maui -f net10.0-maccatalyst  # app desktop
```
O backend (`Taskboard.Server`) e a UI Blazor devem servir no mesmo host/porta
(origem única) para evitar CORS e manter paridade de embed.

## Mapeamento do original → Blazor
| React original | Blazor/.NET MAUI |
|---|---|
| `App.tsx` / `main.tsx` | `App.razor` / `Program.cs` (Blazor) + `MainPage` (MAUI) |
| `api.ts` | `TaskboardClient` (shared `Taskboard.Client`) |
| `revisionPolling.mjs` + SSE | `EventStream` / `RevisionPolling` (C# `HttpResponseMessage` stream) |
| `aiChatState.ts` | `AiChatState` |
| `components/*.tsx` (38) | `Components/*.razor` (cards/colunas/menus) |
| `i18n.tsx` / `workflowI18n.ts` | serviço de localização + resx |
| `storage.ts` | serviço p/ `/api/client-storage` |
| Tauri (`src-tauri`) | .NET MAUI Blazor Hybrid |

## .NET mapping
- Markdown: `Markdig` (GFM) + sanitização (desabilitar raw HTML); mermaid via
  interop JS ou render estático.
- SSE: `HttpClient` + `ReadAsStreamAsync` linha-a-linha; `CancellationToken`
  p/ keep-alive; reconnect com full refresh.
- Componentes: `MudBlazor` acelera cards/diálogos; ou controles MAUI nativos.
- Reuso de `Taskboard.Domain` para tipos e `Taskboard.Client` para transporte HTTP.
