# Spec: AI Chat Subsystem (module `ai-chat`)

Descreve o subsystem de chat AI local do `dashi-taskboard`
(`server/ai-chat.mjs`, `ai-chat-catalog.mjs`, `ai-chat-process.mjs`,
`ai-turn-owner.mjs`, `codex-app-server.mjs`). Permite que a UI inicie conversas
de IA por projeto/issue, que rodam como processos Codex `app-server` e cujos
eventos são transmitidos via SSE por thread.

## Modelo de dados (ver `SPEC-001`)
- `ai_chat_threads`: `id`, `title`, `status`(idle|running|failed),
  `origin_project_id/name/workspace_path`, `origin_issue_id/identifier`,
  `codex_thread_id`, `model`, `reasoning_effort`, `sandbox`
  (read-only|workspace-write|danger-full-access), timestamps.
- `ai_chat_runs`: `id`, `thread_id`, `status`(running|completed|failed|interrupted),
  `exit_code`, `error`, `started_at`, `finished_at`. 1 run ativo por thread.
- `ai_chat_events`: `id`, `thread_id`, `run_id`, `type`, `role`
  (user|assistant|activity|error), `content`, `data`(JSON), `created_at`.

## Endpoints (ver `SPEC-002`)
- `GET/POST /api/local/ai/threads` · `GET/PATCH/DELETE /api/local/ai/threads/:id`
- `GET /api/local/ai/threads/:id/events` (**SSE** `event: ai.run` / `ai.event`)
- `POST /api/local/ai/threads/:id/turns` (202 `{run}`, body ≤25MiB)
- `POST /api/local/ai/threads/:id/compact` · `POST /api/local/ai/runs/:id/interrupt`
- `GET /api/local/ai/catalog` · `GET /api/local/ai/composer/candidates` ·
  `POST /api/local/ai/composer/rebind`

## Codex app-server (`codex-app-server.mjs`)
Classe `CodexAppServer` que spawna `codex app-server --stdio` e fala JSON-RPC
line-delimited via stdin/stdout:
- Handshake: `initialize` → `initialized` notification.
- Métodos RPC: `skills/list`, `thread/start`, `thread/resume`, `turn/start`,
  `turn/interrupt`, `thread/compact/start`.
- `subscribe(listener)` para notificações; `request(method,params)` com
  `requestTimeoutMs` (30s) e buffer cap (4MiB stdout).
- `withoutTaskboardLauncherEnvironment` remove env `CODEX_TASKBOARD_*` do child;
  `executableCommand` roda `.js/.mjs` via `node`.
- `listSkills(workspacePath)`, `startThread`, `startTurn`, `interruptTurn`,
  `compactThread`.

## Catálogo (`ai-chat-catalog.mjs`)
- `loadDeviceWorkspaces(codexStatePath, database)`, `resolveAiWorkspace(projectId,...)`
- `resolveMappedAiWorkspace(projectId, project, projectMappings)`
- `composerCandidatesForSurface(...)`, `ComposerCatalog`
- `loadSlashCommands(platform)`, `discoverAiCatalog({...})`

## Comportamento
- UI abre thread por issue/projeto; `turn/start` envia mensagem + `skillIds`
  + anexos; Codex roda localmente (sandbox definido pela UI); eventos fluem
  por SSE; `interruptTurn` cancela.
- `model`/`reasoning_effort`/`sandbox` configuráveis por thread.
- `latestTodo` (completed/total) derivado dos eventos para a UI.

## .NET mapping (`Taskboard.AiChat`)
- Serviço `CodexAppServer` (.NET) que spawna `codex app-server --stdio` com
  `Process` + `StreamReader/Writer`, implementando o mesmo handshake JSON-RPC.
- Repositório EF/Core para `AiChatThread`/`AiChatRun`/`AiChatEvent`.
- Endpoints Minimal API iguais; SSE por thread (`Channel<AiEvent>` → stream).
- Tempo de vida do processo Codex gerenciado por `IHostedService`/pool.
- **Observação**: depende do binário `codex` externo; o clone mantém a mesma
  integração (não reimplementa o modelo de IA).
