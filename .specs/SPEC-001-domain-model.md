# Spec: Domain Model & Persistence (module `domain-model`, `persistence`)

Descreve o modelo de dados atual do `dashi-taskboard`, fonte para o clone em .NET.
Storage: **SQLite** (`node:sqlite` `DatabaseSync`), modo WAL, `PRAGMA foreign_keys=ON`.
Timestamps em **ISO-8601 string**; IDs em `randomUUID()` (exceto `identifier` humano
`PREFIX-N`). Anexos: bytes em disco em `<dataDir>/attachments/<id>`.

## Entidades (mapear para tabelas/classes .NET)

### `projects`
| coluna | tipo | notas |
|---|---|---|
| `id` | TEXT PK | slug lowercase (`local`, `jira-my-tasks`) |
| `name` | TEXT NOT NULL | |
| `workspace_path` | TEXT NULL | `local` tem NULL |
| `labels` | TEXT JSON array | default `DEFAULT_LABEL_NAMES` |
| `next_task_number` | INTEGER DEFAULT 1 | contador por projeto p/ `identifier` |
| `created_at`/`updated_at` | TEXT NOT NULL | |
Campo virtual `issue_count` (count de tasks não-arquivadas). `source` =
`"jira"` se `id=="jira-my-tasks"` senão `"local"`.

### `tasks` (issues)
`id` UUID PK · `identifier` TEXT UNIQUE (`PREFIX-N`) · `project_id` FK→projects
· `title` ≤240 NOT NULL · `description` TEXT DEFAULT '' · `status` enum
`backlog|todo|in_progress|in_review|blocked|done|canceled` · `priority`
`none|urgent|high|medium|low` · `labels` JSON array (≤20 strings, 1–64 chars)
· `sort_order` REAL (novo = `min-1000`) · `thread_id` · `thread_codex_project_id`/
`thread_codex_project_kind`(`local`|`remote`)/`thread_codex_host_id`/
`thread_workspace_path` · `creator_type`(`user`|`agent`) · `creator_id`
DEFAULT `local-user` · `creator_name` DEFAULT `本地用户` · `creator_avatar_url` ·
`assignee_type`/`assignee_id`/`assignee_name`/`assignee_avatar_url` ·
`workflow_id` · `git_branch` · `worktree_path` · `worktree_branch` ·
`start_date` · `due_date` (REQUIRED se recurrence) · `recurrence_interval` INTEGER
· `recurrence_unit`(`day|week|month|year`) · `external_source`(`'jira'`)/
`external_origin`/`external_id`/`external_key`/`external_url` · `archived_at` ·
`version` INTEGER DEFAULT 1 CHECK>0 (**concorrência otimista**) ·
`created_at`/`updated_at`.

Campos computados (serializar na API):
- `developmentContext`: `{type:'worktree',path,branch}` | `{type:'branch',branch}` | null.
- `source`: `jira` se `external_source=='jira'` senão `local`.
- `recurrence`: `{interval,unit}` | null.
- `threadBinding`: `{threadId,codexProjectId,codexProjectKind,codexHostId,workspacePath}`
  se os 5 presentes; senão `legacyLocalThreadId` = `thread_id` puro.
- `relations`: `parent` (task summary), `subIssues[]`, `blockedBy[]`, `blocks[]`, `related[]`.

### `comments`
`id` UUID PK · `task_id` FK→tasks ON DELETE CASCADE · `body` NOT NULL ·
`thread_id` + 4 campos de `threadBinding` · `author_type`(`user`|`agent`) ·
`author_id`/`author_name` NOT NULL · `author_avatar_url` · `version` INTEGER
DEFAULT 1 CHECK>0 · `created_at`/`updated_at`. Computados: `threadBinding`/
`legacyLocalThreadId`, `attachments[]`.

### `task_activities` (audit log)
`id` · `task_id` FK→tasks CASCADE · `actor_type`(`user`|`agent`) ·
`actor_id`/`actor_name` · `actor_avatar_url` · `changes` JSON array
`{field,before,after}` · `created_at`.

### `attachments`
`id` UUID PK · `task_id` FK→tasks CASCADE · `comment_id` FK→comments CASCADE NULL ·
`kind` CHECK(`inline`|`attachment`) · `filename` NOT NULL · `content_type` NOT NULL
· `size` INTEGER ≥0 · `created_at`. Bytes em disco (`inline` inferido quando
imagem embutida no body).

### `task_relations` (join)
PK = `(relation_type, source_task_id, target_task_id)`. `relation_type`
(`parent`|`blocks`|`related`) · `source_task_id` FK · `target_task_id` FK ·
`created_at`. Regras: `source != target`; `related` exige `source<target`;
UNIQUE `task_relations_one_parent` (1 parent por target). `parent` é armazenado
invertido (child = source, parent = target).

### `workflow_workspaces`
`project_id` PK FK · `workspace` TEXT JSON (grafo arbitrário) · `version` INTEGER
DEFAULT 1 · `updated_at`.

### `project_summaries`
`project_id` PK · `summary` TEXT NULL · `generated_at`/`attempted_at` · `error`.

### `ai_chat_threads`
`id` PK · `title` NOT NULL · `status`(`idle`|`running`|`failed`) ·
`origin_project_id`/`origin_project_name`/`origin_workspace_path` NOT NULL ·
`origin_issue_id`/`origin_issue_identifier` · `codex_thread_id` · `model` NOT NULL
· `reasoning_effort` NOT NULL · `sandbox`(`read-only`|`workspace-write`|
`danger-full-access`) · `created_at`/`updated_at`. Computados: `origin{...}`,
`currentRun`, `latestTodo{completed,total,eventId,updatedAt}`.

### `ai_chat_runs`
`id` PK · `thread_id` FK→ai_chat_threads CASCADE · `status`
(`running`|`completed`|`failed`|`interrupted`) · `exit_code` INTEGER · `error` ·
`started_at` NOT NULL · `finished_at`. UNIQUE parcial `ai_chat_runs_one_active`
(1 `running` por thread).

### `ai_chat_events`
`id` PK · `thread_id` FK · `run_id` FK · `type` NOT NULL · `role`
(`user`|`assistant`|`activity`|`error`) · `content` NOT NULL · `data` JSON NULL
· `created_at`.

## Relacionamentos
tasks→project · comments/activities/attachments→task (CASCADE) · task_relations
→task×2 · workflow_workspaces & project_summaries→project · ai_chat_runs/events→
ai_chat_threads.

## Migrações (paridade `cloud/migrations/*.sql`)
1. `0001_initial` (projects, tasks, comments, attachments, relations)
2. `0002_add_start_date`
3. `0003_global_project`
4. `0004_task_activities`
5. `0005_task_relation_project_integrity`
6. `0006_project_labels`
7. `0007_thread_identity` (5 colunas thread_*)
8. `0008_attachment_kind`
No .NET: scripts SQL idempotentes applyados na startup (CREATE TABLE IF NOT EXISTS
+ ALTER ADD COLUMN ignorando duplicado), igual ao `#migrate()` do original.

## Enums (manter nomes)
`TASK_STATUSES = [backlog, todo, in_progress, in_review, blocked, done, canceled]`
`TASK_PRIORITIES = [none, urgent, high, medium, low]`
`DEFAULT_PROJECT_ID = "local"` · `JIRA_PROJECT_ID = "jira-my-tasks"`
`DEFAULT_LABEL_NAMES` (12): 缺陷, 特性, for-claude, hold, 改进, phase-1..6.

## .NET mapping
- `Taskboard.Domain`: classes `Project`, `Task`, `Comment`, `TaskActivity`,
  `Attachment`, `TaskRelation`, `WorkflowWorkspace`, `ProjectSummary`,
  `AiChatThread`, `AiChatRun`, `AiChatEvent`; records computados
  `ThreadBinding`, `DevelopmentContext`, `TaskRelations`, `Recurrence`.
- `Taskboard.Persistence`: `DbContext` (EF Core) **ou** `SqliteConnection` +
  DAL. Recomendado **raw `Microsoft.Data.Sqlite`** p/ paridade exata das migrações
  e do `version` optimistic lock (`UPDATE ... WHERE id=@id AND version=@version`).
- Anexos: `<DataDir>/attachments/<id>` (sem DB). `DataDir` default `.data`,
  override via `CODEX_TASKBOARD_DATA_DIR`.
