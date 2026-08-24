# MEMORY.md — State Management

## Decisões Técnicas

| Data | Decisão | Motivo | Alternativas Descartadas |
|---|---|---|---|
| 2026-08-24 | .NET 10 / C# 14 | Alinhado com .NET unification | Manter Node.js original |
| 2026-08-24 | ABP N-Layer DDD | Convenções afonsoft | Clean Architecture pura |
| 2026-08-24 | EF Core + SQLite | Local-first, portátil | PostgreSQL (muito pesado) |
| 2026-08-24 | Minimal APIs | Simplicidade e performance | Controllers tradicionais |

## Débitos Técnicos

| Item | Impacto | Prioridade |
|---|---|---|
| Persistência real de anexos | Médio | Média |
| Integração LLM real | Alto | Média |
| UI Blazor/MAUI | Alto | Baixa (fase 2) |

## Lições Aprendidas

| Contexto | Erro | Como Evitar |
|---|---|---|
| Specs | Duas pastas `.specs` e `.specs2` causaram confusão | Unificar via merge e SDD |

## Políticas de Limpeza

- Memórias de branches deletadas devem ser descartadas.
- Fatos desatualizados devem ser removidos.
- Nunca armazenar PII, secrets ou credenciais.

## Tiers de Memória

| Tier | Persistência | Conteúdo | Implementação |
|---|---|---|---|
| Procedural | Sempre | Como trabalhar | CLAUDE.md, rules |
| Semantic | Sob demanda | Fatos, padrões | `.specs/`, `docs/`, `.claude/knowledge/` |
| Episodic | Cross-session | Experiências | MEMORY.md |
