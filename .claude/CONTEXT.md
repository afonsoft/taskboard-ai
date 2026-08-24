# CONTEXT.md — Context Engineering

## Estratégias de Carregamento

| Tipo | Quando | Exemplos |
|---|---|---|
| Always-on | Sempre carregado | `CLAUDE.md`, `.claude/rules/global-rules.md` |
| Pattern-matched | Por tipo de arquivo | `.claude/rules/*.md` com `paths:` |
| On-demand | Quando solicitado | `.specs/`, `docs/`, `.claude/knowledge/` |
| Progressive disclosure | Codebase grande | Mapa de dirs → headers → conteúdo |

## Token Budget

- Reservar 20% do contexto para output.
- Arquivos >500 linhas devem ser chunkados por read com offset/limit ou resumidos.
- Specs longos podem ser lidos sob demanda (`SPEC-000-overview.md`, `CAPABILITY-MAP.md` prioridade).

## Context Compaction

1. budget reduction — remover histórico irrelevante
2. snip — omitir trechos que não mudam decisão
3. microcompact — resumir specs já entendidos
4. collapse — manter apenas nomes de arquivos quando conteúdo for inferível
5. auto-compact — quando acima de 80%, sumarizar

## Hierarquia de Prioridade

1. CLAUDE.md
2. .claude/rules/global-rules.md
3. .specs/CAPABILITY-MAP.md
4. Spec relevante à tarefa
5. docs/ technologies.md, features.md, api.md
6. Código fonte (src/)

## Checklist

- [ ] Carregar CLAUDE.md e rules sempre
- [ ] Carregar spec alvo antes de implementar
- [ ] Não inventar contexto — evideNCiar pelo repo
- [ ] Atualizar MEMORY.md após conclusão
