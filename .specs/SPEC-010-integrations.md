# Spec: Integrations (module `integrations`)

Descreve as integrações externas do `dashi-taskboard`: **Jira**
(`server/jira-config.mjs`, `server/jira-integration.mjs`) e o **DeepSeek harness**
(`integrations/deepseek-harness/`). Também cobre módulos compartilhados de
execução de processos (`shared/*.mjs`) usados pelas integrações e pelo AI chat.

## Jira (`jira-config.mjs` / `jira-integration.mjs`)
- Projeto especial `jira-my-tasks` (`JIRA_PROJECT_ID`): `source:"jira"`,
  issues com `external_source:'jira'`, `external_id/key/url/origin`.
- Endpoints (ver `SPEC-002`): `GET/PUT /api/local/jira-connection`
  (`{baseUrl,username,password,projects}`), `POST /api/local/jira-connection/sync`
  (vazio → `{connection}`).
- `jira-config.mjs` persiste credenciais/mapeamento; `jira-integration.mjs`
  sincroniza issues Jira ↔ tasks locais (cria/atualiza tasks espelhando o Jira).
- Tasks Jira preservam `identifier`/`external_key`; labels JIRA protegidas contra
  delete (409 em `POST/DELETE /api/projects/:id/labels`).

## DeepSeek harness (`integrations/deepseek-harness/`)
- Plugin/adaptador para ecossistema DeepSeek usar o mesmo `taskctl` CLI.
- Reutiliza a API HTTP/MCP existente (não duplica lógica de domínio).

## Módulos de execução compartilhados (`shared/*.mjs`)
- `codex-executable.mjs` — `resolveCodexExecutable({explicit,appPath,env,platform,homeDirectory})`:
  env `CODEX_EXECUTABLE` → bundled-in-app → PATH `codex` → macOS
  `/Applications`/`~/Applications` (`ChatGPT.app`/`Codex.app`) → fallback `"codex"`.
- `process-tree.mjs` — `signalProcessTree(child, signal)`: mata árvore (Windows
  `taskkill /PID /T /F`; POSIX `kill(-pid)` + `child.kill`).
- `codex-environment.mjs` — `withoutTaskboardLauncherEnvironment(env)`: remove
  todos os `CODEX_TASKBOARD_*` do env do child.
- `executable-command.mjs` — `executableCommand(exe,args)`: se `.cjs/.js/.mjs`,
  roda via `node` (`{executable: process.execPath, args:[exe,...]}`); senão passthrough.
- `domain.mjs` — enums (ver `SPEC-001`).
- `workflow-control-flow.mjs` / `workflow-sequence.mjs` — ver `SPEC-007`.
- `taskboard-automation*.mjs` — ver `SPEC-007`.

## .NET mapping (`Taskboard.Integrations`)
- **Jira**: serviço `JiraIntegration` que persiste connection em
  `cloud-companion.json`-like store (ou tabela dedicada) e sincroniza para
  `tasks` com `external_source='jira'`. Endpoints `/api/local/jira-connection`
  espelhados. Reuso das entidades de `SPEC-001`.
- **DeepSeek**: adapter fino sobre a API/MCP .NET (sem nova lógica de domínio).
- **Exec infra**: `CodexExecutableResolver` (.NET) com mesma ordem de resolução;
  `ProcessTreeSignaler` (Kill(whole process tree) no Windows/Linux);
  `WithoutTaskboardEnv` helper; `ExecutableCommand` wrapper.
- Credenciais Jira/cloud: nunca logar nem commitar; modo `0600` no arquivo local.
