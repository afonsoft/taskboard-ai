# Follow-ups — pendências que não bloqueiam mas não podem cair no esquecimento

## Migração CLI (Spectre.Console.Cli) — restos da sessão 2026-08-31

### Bloqueantes (fazer antes de declarar a migração concluída)

- [ ] **Remover duplicata de `context:current`** em `src/Taskboard.Cli/Program.cs`. Existe nas linhas 17 e 43; deletar a linha 43. Spectre lança em nomes de comando duplicados.
- [ ] **Smoke tests:** `--help`, `project:list`, `issue:get <id>`, `cloud:status`.
- [ ] **Commit + push** (ver mensagem sugerida em `cli-migration.md`).

### Não bloqueantes

- [ ] **Testes de CLI com `CommandAppTester`** — cobrir `--help` de cada grupo de comandos para regressar o bug do `CommandArgument`.
- [ ] **Check de CI** para `[CommandArgument]` sem `<>`/`[]` (ver `gotchas.md`, seção "Prevenção").
- [ ] **Comentário inline** em `Program.cs` acima do primeiro `CommandArgument` registrando a convenção de colchetes.
- [ ] **Decisão/ADR** registrando *por que* Spectre foi escolhido em vez de `System.CommandLine`.
- [ ] **Revisar UX diff** com a CLI original — confirmar que nenhum comando foi perdido e que mudanças posicional → `--flag` foram intencionais.

## Convenção para este arquivo

Cada item deve ter: `- [ ]` checkbox, contexto mínimo de *onde* e *por que*, e dono se aplicável. Itens concluídos viram `- [x]` e são limpos na próxima sessão.