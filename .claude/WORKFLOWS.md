# WORKFLOWS.md — Automação

## Workflow de Implementação

### Trigger

Issue, PR, comentário, ou prompt do usuário.

### Passos

1. **Plan** — `.claude/agents/plan.md` para tarefas complexas.
2. **Guardrails** — verificar `CLAUDE.md`, `.claude/rules/global-rules.md`, `.devin/config.json`.
3. **Implement** — editar/código mínimo.
4. **Lint** — `dotnet build`, `dotnet format`.
5. **Test** — `dotnet test`.
6. **Review** — `.claude/agents/review.md`.
7. **CI** — GitHub Actions.
8. **LLM Judge** — opcional, verificação cruzada.
9. **Human** — aprovação de PR.

## Verification Loop

```
Agent Output → Lint → Tests → CI → Review → Merge
```

## Rollback

- Reverter branch via `git revert`.
- Restaurar backup do SQLite.
- Desabilitar feature flag se houver.

## gh-aw (futuro)

Quando adotar GitHub Agentic Workflows:
- Safe-outputs para operações de escrita.
- Context expressions sanitizadas.
- Tool allow-listing.
