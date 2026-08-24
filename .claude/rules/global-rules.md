---
name: taskboard-ai-global
---

# taskboard-ai — Global Rules

> Compatível com Claude Code e Devin CLI.

## Escopo do Agent

- Implementar, revisar e documentar o `taskboard-ai` em C# 14 / .NET 10 seguindo as specs em `.specs/`.
- Criar branches, commits e PRs; nunca push direto em branches protegidas.

## Hard Rules

1. **Branches protegidas**: jamais commit/push em `main`, `master`, `develop`.
2. **Workflows protegidos**: não modificar `/.github/workflows/**` sem aprovação humana.
3. **Secrets**: nunca logar, commitar ou expor tokens, senhas ou API keys.
4. **Specs**: toda mudança de contrato/arquitetura deve ser refletida em `.specs/`.
5. **Tests**: features/bugfixes precisam de testes (unit/integration).
6. **Coverage**: manter ≥80% (meta 90%).
7. **Build**: `dotnet build` com `TreatWarningsAsErrors`.
8. **Don't**: não criar `DEVIN.md`, `AGENTS.md`, `.cursorrules`, `.geminiignore`, etc.

## Soft Rules

1. Modificar `common.props` → avisar no PR.
2. Adicionar pacote NuGet → justificar no PR.
3. Mudar rota HTTP → documentar breaking change.

## Planejamento Obrigatório

Antes de qualquer modificação, apresentar Execution Plan com:
1. Goal and context
2. Impacted files/modules
3. Implementation strategy
4. Risks and mitigations
5. Validation steps (build, test, lint)
6. Rollback plan

## Comportamento

- Código e commits em inglês; testes BDD e documentação podem ser em português.
- Prefira minimal changes; não refatore sem necessidade.
- Execute `dotnet build` e `dotnet test` após alterações relevantes.
