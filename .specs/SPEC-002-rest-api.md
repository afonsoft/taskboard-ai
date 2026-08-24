# Spec: REST API (module `rest-api`)

Descreve o servidor HTTP atual do `dashi-taskboard` (`server/app.mjs`): servidor
`node:http` customizado, roteamento manual por `url.pathname` (sem framework de
rotas), sob prefixo `/api/`. Fonte para o clone ASP.NET Core Minimal APIs.

## Transporte & middlewares
- `createTaskboardServer()` → `http.createServer`. Lista em `0.0.0.0:47823`
  (override `CODEX_TASKBOARD_HOST`/`PORT`). `GET /health` público.
- **CORS**: só `TRUSTED_EMBED_ORIGINS` + `origin==='null'` quando instance-token presente.
- **Auth (LAN/local)**: instance token via headers `x-codex-taskboard-challenge`
  + `x-codex-taskboard-proof` (HMAC-SHA256 do challenge com `instanceSecret`).
- **Loopback obrigatório** para `/api/local/*` e `/api/local/ai/*`
  (`assertLoopbackRequest`/`assertAiLoopbackRequest`).
- **Cloud**: se `cloudConfig.remoteUrl` setado, rotas `/api/` não-companion são
  **proxied** para upstream via `cloudProxy.forward` (modo nuvem não faz fallback local).
- **Actor resolution** (`actorFromRequest`): header `x-taskboard-client: taskctl`
  → ator agente (`CODEX_AGENT_ACTOR`); senão `x-taskboard-user-id`/`-name`/`-avatar`
  (URL-encoded, id validado por regex); ausente → default `local-user`/`本地用户`.

## Roteamento (tabela completa)

| Method | Path | Query | Body | Response |
|---|---|---|---|---|
| GET | `/health` | — | — | `{status:"ok"}` (+`product,version,proof` c/ token) |
| GET/PUT/PATCH | `/api/client-storage` | — | PATCH: merge JSON | GET `{entries}`; PATCH 204 |
| GET | `/api/local/codex-thread-progress` | `threadId` | — | `{progress}` |
| GET/PUT | `/api/local/host-runtime` | — | `{threadId,threadRunning,threadTodoProgress,codexProjectId,codexProjectKind,codexHostId,workspacePath}` | `{runtime}` |
| GET/PUT/DELETE | `/api/local/cloud-session` | — | PUT `{remoteUrl,actorName,sharedKey}` | `{mode,remoteUrl?,actorName?,authenticated}` |
| GET/PUT | `/api/local/jira-connection` | — | PUT `{baseUrl,username,password,projects}` | `{connection}` |
| POST | `/api/local/jira-connection/sync` | — | vazio | `{connection}` |
| PUT | `/api/local/project-mappings/:projectId` | — | `{workspacePath}` | `{projectId,workspacePath}` |
| GET | `/api/meta` | — | — | `{manageTaskboardSkillPath,capabilities:{localAiChat},mode,realtime:{transport:'poll',intervalMs:2000},localCapabilities}` |
| GET | `/api/local/ai/catalog` | `projectId` | — | catalog |
| GET | `/api/local/ai/composer/candidates` | `projectId,threadId,trigger('/'\|'@'),query,surface` | — | candidates |
| POST | `/api/local/ai/composer/rebind` | — | `{contractVersion:'composer.v1',projectId,threadId,document}` | refs |
| GET | `/api/local/projects/:id/summary` | — | — | `{projectId,summary,generatedAt,attemptedAt,error}` |
| GET/POST | `/api/local/ai/threads` | — | POST `{projectId,issueId,title,model,reasoningEffort,sandbox}` | GET `{threads}`; POST 201 `{thread}` |
| GET | `/api/local/ai/threads/:id/events` | — | — | **SSE** `ai.run`/`ai.event` |
| POST | `/api/local/ai/threads/:id/turns` | — | `{message,skillIds,dangerFullAccessConfirmed,attachments[]}` (≤25MiB) | 202 `{run}` |
| POST | `/api/local/ai/threads/:id/compact` | — | vazio | `{thread}` |
| GET/PATCH/DELETE | `/api/local/ai/threads/:id` | — | PATCH `{title,model,reasoningEffort,sandbox}` | `{thread}`; DELETE 204 |
| POST | `/api/local/ai/runs/:id/interrupt` | — | vazio | `{run}` |
| GET | `/api/device-workspaces` | — | — | `{workspaces}` |
| GET | `/api/workflow-capabilities` | `workspacePath` | — | capabilities |
| GET/POST | `/api/projects` | — | POST `{id?,name,workspacePath?}` | GET `{projects}`; POST 201 `{project}` |
| DELETE | `/api/projects/:id` | — | — | 204 (só `temp-*` deletável; 409 se tem tasks; 403 c/c) |
| POST/DELETE | `/api/projects/:id/labels` | — | `{label}` | `{project}` (409 deletando label JIRA) |
| GET/PUT | `/api/projects/:id/workflow-workspace` | — | PUT `{version,workspace}` | `{workflow}` |
| GET | `/api/projects/:id/development-contexts` | `codexProjectId,codexThreadId,workspacePath` | — | contexts |
| GET/POST | `/api/tasks` | GET `projectId,status,archived('true'\|'false'\|'all')` | POST create (abaixo) | GET `{tasks}`; POST 201 `{task}` |
| GET/PUT/DELETE | `/api/tasks/:id` | — | PATCH `{version,...}`; DELETE `{version}` | GET `{task}`; PATCH `{task}`; DELETE 204 |
| POST | `/api/tasks/:id/move` | — | `{version,status,sortOrder?,threadId?,threadBinding?}` | `{task}` |
| POST | `/api/tasks/:id/archive` · `/restore` | — | `{version,threadId?,threadBinding?}` | `{task}` |
| POST/DELETE | `/api/tasks/:id/relations/:type/:relatedId` | — | `{version,threadId?,threadBinding?}`; `type∈parent\|blocks\|blocked_by\|related` | `{task,relatedTask}` |
| GET | `/api/tasks/:id/activities` | — | — | `{activities}` |
| GET/POST | `/api/tasks/:id/comments` | — | POST `{body,threadId?,threadBinding?}` | GET `{comments}`; POST 201 `{comment}` |
| PATCH/DELETE | `/api/comments/:id` | — | PATCH `{version,body}`; DELETE `{version}` | `{comment}`; DELETE 204 |
| GET/POST | `/api/comments/:id/attachments` · `/api/tasks/:id/attachments` | — | POST raw bytes + headers | `{attachments}`/`{attachment}` |
| GET/HEAD | `/api/attachments/:id/content`·`/download` | — | — | bytes; `inline` se `content`+image |
| DELETE | `/api/attachments/:id` | — | — | 204 |
| GET | `/api/events` | — | — | **SSE** global |

## Task create body (`parseTaskCreate`)
`projectId`(def `local`), `title`(≤240,req), `description`(≤100k),
`status`(def `backlog`), `priority`(def `none`), `labels`(≤20), `sortOrder`,
`threadId`, `threadBinding`, `assigneeTarget`(`current-user`|`codex-agent`),
`workflowId`, `developmentContext`(`{type:'branch',branch}`|
`{type:'worktree',path,branch}`), `startDate`, `dueDate`,
`recurrence`(`{interval,unit}`).

## Task PATCH body (`parseTaskPatch`)
`version`(req) + qualquer de: `projectId,title,description,status,priority,
labels,workflowId,developmentContext,startDate,dueDate,recurrence,assigneeTarget`.
`?recurrence` exige `dueDate`.

## `threadBinding`
Todas-ou-nenhuma: `{threadId,codexProjectId,codexProjectKind('local'|'remote'),
codexHostId,workspacePath}`. Se só `threadId` → `{threadId}` (legacy local).
`resolveInputThreadBinding` injeta host binding do contexto da requisição quando
`threadBinding` omitido.

## SSE — EventHub
- `/api/events`: `EventHub` com `clients:Set<response>`. `connect()` seta
  `text/event-stream`, 20s keep-alive (`: keep-alive`). `emit(type,value)` →
  `{type, projectId, taskId, ...value, at:ISO}`.
  Eventos: `project.created`, `project.labels.updated`, `workflow.updated`,
  `task.created/updated/moved/archived/restored/deleted`, `task.relation.updated`,
  `comment.created/updated/deleted`, `attachment.created/deleted`.
- `/api/local/ai/threads/:id/events`: SSE por thread via `aiChat.subscribe`.
- **Cloud realtime**: polling-only (`/api/meta` → `realtime:{transport:'poll',intervalMs:2000}`).

## Concorrência otimista
Toda mutação em `tasks`/`comments` exige `version` (int >0). DB:
`UPDATE ... WHERE id=? AND version=?`; mismatch/row gone → `ApiError(409,
"VERSION_CONFLICT", {expectedVersion, actualVersion})`. `updateTask` faz
`version=version+1` e grava `task_activities`. `workflow-workspace` tem seu
próprio `version` com mesmo 409. CLI passa `--if-version` ou lê `version` antes.

## .NET mapping
- `WebApplication` + `MapGet/Post/...` em grupos (`/api/projects`, `/api/tasks`,
  `/api/local/ai/threads`, etc.). Middleware de CORS + instance-token.
- SSE: `Response.ContentType="text/event-stream"`; loop com `CancellationToken`;
  hub `ConcurrentDictionary`/Channel para `emit`.
- Erros: middleware central → `{ error:{ code, message } }` + status; 409 p/
  `VERSION_CONFLICT` via exceção de domínio `VersionConflictException`.
- Validação estrita de query params (`assertAllowedQuery`) → 400
  `UNKNOWN_QUERY_PARAMETER`.
