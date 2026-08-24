# Spec: Skills (module `skill`)

Descreve a skill `manage-taskboard` (`skills/manage-taskboard/SKILL.md` +
`references/cli.md`) e a skill de automação. No clone .NET, a skill permanece
**markdown portada 1:1**; a invocação fica a cargo do host de agente
(OpenCode, Claude Code, Cursor, etc.) que consome a nova API/MCP.

## `manage-taskboard` SKILL.md
- **Frontmatter**: `name: manage-taskboard`; `description` cobre issue IDs,
  status sync, comments, cloud setup, e suporte MCP universal.
- **Princípio central**: use `taskctl` para TODA operação de project/issue/
  relation/comment; consuma JSON; use o `identifier` exato retornado (nunca
  adivinhe/reescreva prefixo).
- **Seleção de CLI/service**: use o binário `taskctl` exato e URL injetados;
  no macOS app, `'/Applications/Codex Taskboard.app/Contents/Resources/bin/taskctl'`.
- **Terminologia "companion"**: serviço loopback device-local (auth Basic,
  mapeamento de paths, Codex/Git/Skill/MCP). NÃO traduzir como "伴侣".
- **Core workflow** (estende regras Codex):
  1. Para issue existente: `issue get` + `comment list`; leia antes de decidir.
  2. `backlog` = não aprovado; não claim sem autorização. Claim `todo`→`in_progress`
     com `version` atual; pare se já bound a outra conversa.
  3. Conflito de `version` → releia e retry uma vez se ainda claimable; senão pare.
  4. `context current` p/ matching de workspace; `project list` p/ selecionar
     projeto exato; atualize issue existente em vez de duplicar.
  5. Execute no branch/worktree bound.
  6. Verifique; comente mudanças + resultado; `move` para `in_review` com `version`.
  7. `done` só após usuário aceitar explicitamente. `blocked`/`canceled` conforme.
- **Bindings**: `CODEX_THREAD_ID` (Codex) vs `TASKBOARD_THREAD_ID` (outros CLIs,
  prioridade maior). Git/worktree via `--git-branch`/`--worktree-*`.
- **AI integrations**: Claude (`--thread-id`/`TASKBOARD_THREAD_ID`), OpenCode/
  Cursor/Gemini (`TASKBOARD_THREAD_ID`), Devin/WorkBuddy/DeepSeek (plugin),
  CI (`TASKBOARD_URL`). MCP server disponível.
- **Outros**: preserve scope; relações `parent`/`blocks`/`related`; `--if-version`
  p/ updates concorrentes; baixe inline images só se necessário.

## Skill de automação (Codex)
- Gerada por `buildTaskboardAutomation*` (`SPEC-007`): cron que claim/dispatch
  `todo` para sessões Codex remotas via SSH, com handoff de `threadBinding`.
- Instrução em chinês; `rrule` MINUTELY com `interval`.

## `references/cli.md`
Catálogo de sintaxe de comandos `taskctl` (ver `SPEC-003`); a skill abre só a
seção relevante.

## .NET mapping
- **SKILL.md**: copiar conteúdo 1:1 para `skills/manage-taskboard/SKILL.md` no
  repo clone; ajustar apenas paths de exemplo (`taskctl` → `dotnet run --project
  Taskboard.Cli --` ou binário empacotado) sem mudar o fluxo. Manter `name`
  e `description` idênticos p/ descoberta pelos hosts.
- `references/cli.md`: regenerar a partir de `SPEC-003` (comandos .NET).
- A invocação/execução da skill é responsabilidade do host de agente, não do
  serviço .NET. O serviço expõe a mesma API/MCP para que a skill funcione.

## Configuração de host (compatível)
- OpenCode: symlink `skills/manage-taskboard` → `~/.opencode/skills/`; `TASKBOARD_THREAD_ID`.
- Claude Code: `~/.claude/skills/`; MCP em `claude_desktop_config.json`.
- Cursor: `~/.cursor/skills/`; MCP em `~/.cursor/mcp.json`.
