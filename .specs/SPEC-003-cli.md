# Spec: CLI `taskctl` (module `cli`)

Descreve o `cli/taskctl.mjs` (1112 linhas, ESM puro, sem deps). Cliente de linha
de comando que fala HTTP com o serviço. Fonte p/ o clone `Taskboard.Cli` (.NET
console). Deve manter paridade de comandos, flags, envelope JSON e exit codes.

## Comportamento base
- `SCHEMA_VERSION=2`; `DEFAULT_API_URL="http://127.0.0.1:47823"`.
- Saída: envelope JSON `{...result, schemaVersion}` no **stdout**; erros no
  **stderr** como `{schemaVersion, error:{code,message,details?}}`.
- `--json` aceito (sempre JSON aqui). Exit codes: `0` ok · `1` INTERNAL ·
  `2` USAGE/IO · `3` SERVICE_UNAVAILABLE · `4` INVALID_RESPONSE · `5` HTTP 409.
- Resolução da URL base:
  1. `CODEX_TASKBOARD_URL` (validado http/https)
  2. `.data/launcher-runtime.json` (`CODEX_TASKBOARD_RUNTIME_FILE` override) →
     `descriptor.url` (deve ser v1)
  3. `DEFAULT_API_URL`
- Para `cloud *` e `project map` → `resolveCompanionUrl` (loopback http/https,
  token path `^[a-z0-9-]{16,128}$`); usa `CODEX_TASKBOARD_COMPANION_URL` se set.
  `--runtime-file` global troca o descritor de runtime.

## Cliente HTTP (`createApiClient`)
`fetch` com headers `accept: application/json`, `x-taskboard-client: taskctl`,
`content-type` p/ bodies.

## Tabela de comandos (paridade exata)

| Comando | Flags | Comportamento |
|---|---|---|
| `project list` | `--json` | `GET /api/projects` |
| `project create` | `--id --name --workspace-path --json` | `POST /api/projects` |
| `project map <id>` | `--workspace-path --json` | `PUT /api/local/project-mappings/:id` |
| `cloud login` | `--url --actor-name --json` | lê shared key do stdin (TTY/raw); `PUT /api/local/cloud-session` |
| `cloud status` | `--json` | `GET /api/local/cloud-session` |
| `cloud logout` | `--json` | `DELETE /api/local/cloud-session` |
| `issue list` | `--project --status --archived(true\|false\|all) --json` | `GET /api/tasks` |
| `issue get <id>` | `--json` | `GET /api/tasks/:id` |
| `issue create` | `--project --title --description --description-file --status --priority --labels(csv) --thread-id --git-branch --worktree-path --worktree-branch --start-date --due-date --recurrence-interval --recurrence-unit --json` | `POST /api/tasks` |
| `issue update <id>` | (acima) + `--if-version` | `PATCH /api/tasks/:id` |
| `issue move <id>` | `--status --thread-id --binding-thread-id --binding-codex-project-id --binding-codex-project-kind(local\|remote) --binding-codex-host-id --binding-workspace-path --clear-binding-thread --if-version --json` | `POST /api/tasks/:id/move` |
| `issue archive <id>` / `restore <id>` | `--thread-id --if-version --json` | `POST /api/tasks/:id/archive` / `/restore` |
| `issue relation <add\|remove> <id> <relatedId>` | `--type(parent\|blocks\|blocked_by\|related) --thread-id --if-version --json` | `POST`/`DELETE /api/tasks/:id/relations/:type/:relatedId` |
| `comment list <id>` | `--json` | `GET /api/tasks/:id/comments` |
| `comment add <id>` | `--body --thread-id --binding-* --clear-binding-thread --json` | `POST /api/tasks/:id/comments` |
| `comment update <id>` | `--body --thread-id --if-version --json` | `PATCH /api/comments/:id` |
| `comment delete <id>` | `--thread-id --if-version --json` | `DELETE /api/comments/:id` |
| `attachment download <id>` | `--output --json` | `GET /api/attachments/:id/content` → grava bytes |
| `attachment upload` | `--file --task <id> \| --comment <id> --content-type --kind(inline\|attachment) --json` | POST raw bytes + headers `x-taskboard-filename`,`x-taskboard-attachment-kind`,`content-type` |
| `context current` | `--cwd --json` | `GET /api/projects`; escolhe projeto cujo `workspacePath` contém cwd (match mais longo); senão `local`; senão primeiro |

## Regras de atribuição
- `thread-id`: `--thread-id` explícito → env `TASKBOARD_THREAD_ID` →
  `CODEX_THREAD_ID` (≤256 chars, requerido p/ atribuição de issue/comment).
- `--if-version` opcional; se omitido, CLI faz `GET` p/ obter `version` atual
  antes de mutar.
- `developmentContext`: `git-branch` **ou** `worktree-path`(+`worktree-branch`).
- `recurrence`: `interval`(1–365)+`unit`(day|week|month|year).

## .NET mapping (`Taskboard.Cli`)
- `System.CommandLine` (ou `CliFx`) com a mesma árvore de comandos.
- `HttpClient` singleton; base URL resolvida na mesma ordem; runtime file lido
  de `.data/launcher-runtime.json`.
- Envelope JSON no stdout, erros no stderr; mesmos exit codes (usar
  `Environment.ExitCode`).
- Streaming de anexo: `HttpClient` + `StreamContent` p/ upload; `ReadAsStreamAsync`
  p/ download.
