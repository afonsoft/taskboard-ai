# SPEC-009: Skill `manage-taskboard`

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Agent Skill manage-taskboard |
| Product / System | taskboard-ai |
| Module / Bounded Context | Agent Skills |
| Change type | Migration |
| Repository | afonsoft/taskboard-ai |
| Suggested branch | `devin/spec-skill-net10` |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

A skill atual `skills/manage-taskboard/SKILL.md` orienta agentes de IA a usar `taskctl` e o MCP server.

### Objective

Criar skill equivalente para o ecossistema .NET 10, seguindo a especificação Agent Skills (agentskills.io) e as convenções `afonsoft/agents-skills`.

### Expected outcome

Pasta `skills/manage-taskboard/` contendo `SKILL.md` com YAML frontmatter, descrição, instruções de uso, e referências ao `taskctl` .NET e MCP server.

### Out of scope

- Instalador multi-IDE (pode reutilizar `install.sh` existente).

---

## 2. Agent Role

> Technical writer + agent skill designer familiarizado com agentskills.io.

---

## 3. Agent Autonomy Level

3

---

## 4. Product Context

### Functional context

A skill permite que agentes (Claude, Devin, OpenCode, Cursor, etc) invoquem `taskctl` e consumam o MCP server.

### Technical context

- SKILL.md atual descreve `taskctl` Node.js.
- Frontmatter com `name` e `description`.

### Relevant stack

- Markdown + YAML
- Agent Skills Specification

---

## 5. Task Definition

### Main task

Escrever `SKILL.md` para a skill `manage-taskboard` orientando agentes a usar `taskctl` .NET e MCP server .NET.

### Subtasks

- YAML frontmatter.
- Seção de instalação (symlink por IDE).
- Uso do `taskctl` .NET.
- Uso do MCP server.
- Terminologia (companion, local companion).
- Variáveis de ambiente `TASKBOARD_URL`, `TASKBOARD_THREAD_ID`.

### Do not do

- Não incluir tokens ou credenciais.

---

## 6. Functional Requirements

### FR-001: YAML frontmatter

```yaml
---
name: manage-taskboard
description: Manage Dashi Taskboard via taskctl .NET and MCP server for AI agents.
---
```

### FR-002: Instruções de uso

- Use `taskctl` .NET para todos os comandos.
- Use `--json`.
- Para agentes, defina `TASKBOARD_THREAD_ID`.
- No macOS, use o path quoted se contiver espaços.

### FR-003: Registro MCP

Exemplos para Claude Desktop, OpenCode, Cursor, Gemini, Devin usando `dotnet run --project src/Taskboard.Mcp`.

### FR-004: Core workflow

1. Para issue existente: `issue get` + `comment list`; leia antes de decidir.
2. `backlog` = não aprovado; não claim sem autorização. Claim `todo`→`in_progress` com `version` atual.
3. Conflito de `version` → releia e retry uma vez se ainda claimable; senão pare.
4. `context current` para matching de workspace; `project list` para selecionar projeto exato.
5. Execute no branch/worktree bound.
6. Verifique; comente mudanças + resultado; `move` para `in_review` com `version`.
7. `done` só após usuário aceitar explicitamente. `blocked`/`canceled` conforme.

---

## 7. Business Rules

- Skill é apenas documentação/diretivas para agentes.
- Não contém lógica.
- Não expõe secrets.
- Terminologia "companion" não traduzir como "伴侣".

---

## 8. Domain Modeling

Não aplica.

---

## 9. Expected Architecture

```text
skills/manage-taskboard/
  SKILL.md
  references/
    cli.md
```

---

## 10-11. API/Application Contracts

Não aplica.

---

## 12. Persistence and Data

Não aplica.

---

## 13. Integrations

- MCP server .NET
- CLI .NET

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Taskctl não encontrado | path errado | skill instrui verificar instalação |
| Resposta não JSON | --json ausente | skill instrui usar --json |

---

## 15. Few-Shot Examples

```markdown
## Example

When asked to create an issue, run:

```bash
taskctl issue create --project local --title "Fix bug" --status todo --priority high --json
```
```

---

## 16. Non-Functional Requirements

- Legível por agentes.
- Sem ambiguidade.
- Referências rápidas abertas por skill.

---

## 17. Mandatory Guardrails

- Não expor tokens.
- Não incluir instruções destrutivas.

---

## 18. Expected Tests

| Test | Validation |
|---|---|
| SKILL.md frontmatter | YAML válido |
| references/cli.md | commands atualizados |

---

## 19. Acceptance Criteria

- [ ] `SKILL.md` com frontmatter.
- [ ] Referências ao `taskctl` .NET.
- [ ] Exemplos de registro MCP.

---

## 20. Implementation Plan

1. Criar `skills/manage-taskboard/`.
2. Escrever `SKILL.md`.
3. Gerar `references/cli.md` a partir de `SPEC-003`.

---

## 21. Rollback Strategy

- Reverter skill para versão anterior.

---

## 22. Risks and Mitigations

| Risk | Impact | Probability | Mitigation |
|---|---|---:|---|
| Agentes usarem taskctl errado | Médio | Média | Exemplos de path e env |

---

## 23. Definition of Done

- [ ] SKILL.md revisado.
- [ ] Referências completas.

---

## 24. Key Reminder

> The SPEC is the contract.

## Pending Questions

1. A skill deve suportar apenas `taskctl` .NET ou também MCP?
2. Deve incluir exemplos de `dotnet tool install`?

## Human Approval Checklist

- [ ] Frontmatter válido.
- [ ] Instruções claras.
