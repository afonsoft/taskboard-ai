# RULES.md — Guardrails

## Hard Rules

1. **Branches protegidas**: nunca commit/push em `main`, `master`, `develop`.
2. **Workflows protegidos**: não modificar `/.github/workflows/**` sem aprovação humana.
3. **Secrets**: nunca logar, commitar ou expor tokens, senhas, API keys.
4. **Specs**: mudanças de contrato/arquitetura devem refletir em `.specs/`.
5. **Tests**: features/bugfixes precisam de testes antes do merge.
6. **Coverage**: ≥80% (meta 90%).
7. **Build**: `dotnet build` com `TreatWarningsAsErrors`.
8. **No legacy files**: não criar `DEVIN.md`, `AGENTS.md`, `.cursorrules`, `.geminiignore`, etc.

## Soft Rules

1. Modificar `common.props`/`Directory.Build.props` → avisar no PR.
2. Adicionar NuGet → justificar no PR.
3. Mudar rota HTTP → documentar breaking change.
4. Deletar specs → aprovação humana.

## Permissões por Ambiente

| Ambiente | Leitura | Escrita | Execução |
|---|---|---|---|
| Dev local | Livre | Confirmar | Sandbox |
| CI/CD | Apenas workflows | Nunca | Autorizado |
| Prod | Nunca (não aplica) | Nunca | Nunca |

## Tool Permissions

- Read-only por padrão.
- Write via gates de aprovação.
- Execução em sandbox com logging.
