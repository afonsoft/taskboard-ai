# Gotchas — conhecimentos não-óvios que custaram tempo de debug

## Spectre.Console.Cli — nomes de `CommandArgument` são placeholder de exibição

**Sintoma:** `--help` (ou qualquer renderização de uso) lança:
`Error: An error occured when parsing template.
       Encountered unexpected character 'X'.
       <nome-do-argumento>
       ^ Unexpected character`
ou, chamando `Markup` diretamente:
`Unhandled exception. System.InvalidOperationException:
   Could not find color or style '<nome>'.
   at Spectre.Console.StyleParser.Parse(String text)`

**Causa raiz:** O segundo parâmetro de `[CommandArgument(0, "<nome>")]` é o **placeholder de exibição** que Spectre renderiza dentro de markup. Um nome "cru" como `"project"` vira `[project]` e é interpretado pelo `StyleParser` como uma cor/estilo — que não existe — e lança.

**Convenção:**
| Formato | Significado | Renderiza |
|---|---|---|
| `"<project>"` | obrigatório | `<project>` |
| `"[project]"` | opcional | `[project]` |
| `"project"` | **broken** | crash |

**Impacto de superfície:** um único `CommandArgument` cru quebra o `--help` top-level **inteiro** do app, porque a listagem de comandos renderiza todos os usos.

**Fix:** envolver todos os nomes em `<>` (obrigatório) ou `[]` (opcional). Aplicado aos 21 `CommandArgument` em `src/Taskboard.Cli/Program.cs`.

**Prevenção:** comentário de uma linha acima do primeiro `CommandArgument` + check de CI por `[CommandArgument(\d+, "[^<\[]` (regex para flagar nomes sem colchete inicial).

**Custo de descoberta:** ~1h numa sessão de migração. Manter este arquivo pesquisável para a próxima.