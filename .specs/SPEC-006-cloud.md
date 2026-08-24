# Spec: Cloud Mode & Companion (module `cloud`)

Descreve o modo nuvem do `dashi-taskboard`: um **companion loopback local**
(`cloud-config.mjs`, `cloud-proxy.mjs`) + deployment **Cloudflare** (Worker +
D1 + R2) para dois colaboradores confiáveis com Basic Auth por senha compartilhada.

## Companion local (`cloud-config.mjs`)
- `createCloudConfigStore({configPath})` persiste `.data/cloud-companion.json`
  (mode `0600`). Estrutura: `{version:1, remoteUrl, actorName, sharedKey, projectMappings:{}}`.
- `normalizeCloudUrl(value)`: deve ser HTTPS origin (loopback HTTP permitido p/
  dev); sem user/pass, path, query, hash. → `url.origin`.
- `validateCredentials(actorName, sharedKey)`: actorName 1–120 chars, sem `:`;
  sharedKey 1–4096 chars.
- `validateProjectMappings`: `projectId` não-vazio, `workspacePath` absoluto.
- Métodos: `read()`, `configure({remoteUrl,actorName,sharedKey})`,
  `clearCloud()`, `setProjectWorkspace(projectId,workspacePath)`.
- Escrita atômica (tmp + rename), `pendingWrite` serializa updates.
- `cloudProxy.forward` encaminha rotas `/api/` não-companion para `remoteUrl`
  quando `cloudConfig.remoteUrl` setado (modo nuvem **não** faz fallback local).

## Fluxo de sessão (endpoints `SPEC-002`)
- `GET /api/local/cloud-session` → `{mode,remoteUrl?,actorName?,authenticated}`
- `PUT /api/local/cloud-session` → `{remoteUrl,actorName,sharedKey}` (login;
  sharedKey lido do stdin no CLI)
- `DELETE /api/local/cloud-session` → logout (volta ao modo local; **não** merge)

## Cloudflare (alvo de deployment, opcional no MVP)
- **Worker** `codex-taskboard` serve UI + API JSON.
- **D1** `codex-taskboard-db` = DB autoritativo de negócio (migrações em
  `cloud/migrations/*.sql` — mesmas tabelas de `SPEC-001`).
- **R2** `codex-taskboard-attachments` = anexos.
- Basic Auth HTTPS em UI/API/attachments; `/health` público.
- Realtime: **poll** de revisão global a cada 2s (`/api/meta` →
  `realtime:{transport:'poll',intervalMs:2000}`); UI refresha após mudança.
- **O que fica local**: paths absolutos de projeto/worktree NUNCA vão à nuvem;
  o companion guarda `projectMappings` apenas no dispositivo.
- Modelo de truste: 1 senha compartilhada; Basic username = nome de ator exibido
  (não identidade verificada). Rotação afeta ambos; sem revogação individual.

## CLI (`SPEC-003`)
- `cloud login --url --actor-name` (sharedKey via stdin)
- `cloud status` · `cloud logout`
- `project map <id> --workspace-path` → `PUT /api/local/project-mappings/:id`

## .NET mapping (`Taskboard.Cloud`)
- `CloudConfigStore` (.NET) lê/escreve `.data/cloud-companion.json` atômico,
  validações idênticas (url/credenciais/mappings).
- Middleware de proxy: quando `RemoteUrl` setado, `HttpClient` encaminha
  `/api/*` para a origem nuvem com Basic Auth header (companion aplica auth +
  mapeia project→workspacePath local).
- Migrações D1 do clone: reusar os mesmos scripts SQL (portar p/ o schema .NET).
- Basic Auth no Worker: fora do MVP backend; documentar como deployment step.
