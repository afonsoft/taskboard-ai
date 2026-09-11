# ESTADO_ORCHESTRATOR

> Arquivo de estado da skill `orchestrator` para o repositório `taskboard-ai`. Ler ao iniciar a sessão; escrever ao final de cada fase.

---

## Sessão

- **iniciado_em**: `2026-09-11 01:20:47 UTC`
- **fase_atual**: `Phase 6 - Finalização`
- **repositorio**: `afonsoft/taskboard-ai`
- **branch_trabalho**: `update/skills-lock`
- **framework**: `afonsoft/skills` instalado via `npx skills add afonsoft/skills` (ver `skills-lock.json`)
- **framework_update_check**: `Unable to check framework updates` (diretório de skills não é um repositório git; catalog local a partir de `skills-lock.json`)

---

## Fase 0 — Preconditions

| Item | Status |
|---|---|
| Git inicializado | ✅ (`/home/ubuntu/repos/taskboard-ai`) |
| Remote `origin` GitHub | ✅ `afonsoft/taskboard-ai` |
| Acesso ao repositório | ✅ (`gh repo view` retornou metadados) |

---

## Fase 2 — Audit do Harness

| Item | Status |
|---|---|
| Git inicializado | ✅ (`/home/ubuntu/repos/taskboard-ai`) |
| Remote `origin` GitHub | ✅ `afonsoft/taskboard-ai` |
| Acesso ao repositório | ✅ (`gh repo view` retornou metadados) |
| `CLAUDE.md` (fonte única) | ✅ |
| `AGENTS.md` (thin reference) | ➖ N/A (`CLAUDE.md` proíbe criar `AGENTS.md`) |
| `.claude/settings.json` | ✅ |
| `.claude/rules/global-rules.md` | ✅ |
| `.claude/agents/` (review, plan, test) | ✅ |
| `.claude/CONTEXT.md`, `RULES.md`, `TOOLS.md`, `WORKFLOWS.md`, `README.md`, `MEMORY.md` | ✅ |
| `.claude/memory/` | ✅ |
| `.specs/` | ✅ SPEC-000..015 |
| `docs/architecture/` | ✅ |
| `docs/agents/` | ➖ N/A (não requerido pelo `CLAUDE.md` do projeto) |
| Skills instaladas (`.claude/skills`) | ✅ |
| Skills instaladas (`.devin/skills`) | ❌ `.devin/` não existe no repo |
| Skills instaladas (`.agent/skills`) | ❌ `.agent/` não existe no repo |
| `.devin/config.json` | ❌ `.devin/` não existe |

---

## GAPs Identificados

| # | ID | Dimensão | Severidade | Descritivo | Tier Risco | Status |
|---|----|----------|------------|------------|------------|--------|
| 1 | `GAP-001` | CI/CD | P2 | Workflow `dotnet.yml` sem cache NuGet, sem `concurrency`, sem `permissions` mínimas, sem SonarCloud, sem CodeQL/Dependabot | T2 Batchável (workflows protegidos — requer aprovação humana) | 🟡 queued |
| 2 | `GAP-002` | Documentação | P4 | Docs não cobriam Kanban GitHub nem orquestração de agentes | T1 Auto | 🟢 done |
| 3 | `GAP-003` | Arquitetura | P3 | Logs de agentes apenas em memória (perdidos em restart) | T2 Batchável | 🟡 queued |
| 4 | `GAP-004` | Arquitetura | P3 | Transporte ACP atual é stdin/stdout de linha; JSON-RPC não implementado | T2 Batchável | 🟡 queued |
| 7 | `GAP-007` | Arquitetura | P2 | `.devin/` e `.agent/` ausentes; `skills-lock.json` existe, mas a configuração Devin CLI e os skills Antigravity não estão no workspace | T2 Batchável | 🟡 queued |

---

## Tarefas (Fase 4 — Fila DAG)

### Tarefas Pendentes

```yaml
- id: TASK-003
  desc: "Melhorar GitHub Actions (cache, concurrency, permissions, SonarCloud, CodeQL, Dependabot)"
  skill: /dotnet-github-actions
  gap_ref: GAP-001
  issue_ref: "https://github.com/afonsoft/taskboard-ai/issues/34"
  spec_ref: ".specs/SPEC-20260911-refine-github-actions.md"
  depends_on: []
  status: done
  concluido_em: "2026-09-11"

- id: TASK-004
  desc: "Persistir AgentLogMessage em SQLite (EF Core)"
  skill: /tdd-spec
  gap_ref: GAP-003
  issue_ref: "https://github.com/afonsoft/taskboard-ai/issues/35"
  spec_ref: ".specs/SPEC-20260911-persist-agent-logs.md"
  depends_on: []
  status: done
  concluido_em: "2026-09-11"

- id: TASK-005
  desc: "Adapter ACP JSON-RPC (stdin/stdout) para agentes que suportam o protocolo"
  skill: /tdd-spec
  gap_ref: GAP-004
  issue_ref: "https://github.com/afonsoft/taskboard-ai/issues/36"
  spec_ref: ".specs/SPEC-20260911-acp-json-rpc.md"
  depends_on: []
  status: done
  concluido_em: "2026-09-11"
```

### Tarefas Concluídas

```yaml
- id: TASK-001
  desc: "Kanban GitHub (Octokit, MudBlazor, labels backlog/in-progress/review/done)"
  skill: /tdd-spec
  gap_ref: ""
  issue_ref: "PR #13"
  spec_ref: ".specs/SPEC-010-integrations.md"
  depends_on: []
  status: done
  concluido_em: "2026-09-10"

- id: TASK-002
  desc: "Orquestração de agentes CLI via ACP + SignalR (SPEC-015)"
  skill: /tdd-spec
  gap_ref: ""
  issue_ref: ""
  spec_ref: ".specs/SPEC-015-agent-orchestration.md"
  depends_on: [TASK-001]
  status: done
  concluido_em: "2026-09-10"
```

---

## Checkpoints de Sanidade

| # | Após Tarefa | Data | Reavaliação Necessária? | Ação Tomada |
|---|-------------|------|------------------------|------------|
| 1 | TASK-002 | 2026-09-10 | Não | build + testes verdes; seguir para docs e PR |

---

## Batch de Execução (Tier T2)

| # | GAP | Tarefa | Skill | Status |
|---|-----|--------|-------|--------|
| 1 | GAP-001 | Atualizar `.github/workflows/dotnet.yml` e adicionar `codeql.yml`, `dependabot.yml` | /dotnet-github-actions | 🟡 pending_approval |

---

## Fase 4 — Novas Tarefas Aprovadas (Sessão 2026-09-10)

As specs aprovadas nesta sessão foram registradas para execução:

### Novas GAPs

| # | ID | Dimensão | Severidade | Descritivo | Tier Risco | Status |
|---|---|---|---|---|---|---|
| 5 | `GAP-005` | Frontend/UX | P2 | Telas de login, configurações e skills (tema dark/light, agentes, skills) | T2 | 🟡 queued |
| 6 | `GAP-006` | DevEx | P4 | Instalador leve `install-cli.sh` em `/usr/local/bin` com alias | T2 | 🟡 queued |

### Novas Tarefas

```yaml
- id: TASK-006
  desc: "Implementar telas de login, configurações e skills (SPEC-20260910-ui-login-settings-skills)"
  skill: /tdd-spec
  gap_ref: GAP-005
  issue_ref: "https://github.com/afonsoft/taskboard-ai/issues/24"
  spec_ref: ".specs/SPEC-20260910-ui-login-settings-skills.md"
  depends_on: []
  status: done
  concluido_em: "2026-09-11"

- id: TASK-007
  desc: "Implementar instalador CLI `install-cli.sh` em /usr/local/bin (SPEC-20260910-install-cli-sh)"
  skill: /tdd-spec
  gap_ref: GAP-006
  issue_ref: "https://github.com/afonsoft/taskboard-ai/issues/25"
  spec_ref: ".specs/SPEC-20260910-install-cli-sh.md"
  depends_on: []
  status: done
  concluido_em: "2026-09-11"
```

### Phase 3 — Issues Criadas

| Epic | Slice | GitHub Issue |
|---|---|---|
| E1 - Refinar GitHub Actions | E1/S1 | #30 / #34 |
| E2 - Persistir logs de agentes | E2/S1 | #31 / #35 |
| E3 - Adapter ACP JSON-RPC | E3/S1 | #32 / #36 |
| E4 - Completar harness Devin CLI e Antigravity | E4/S1 | #33 / #37 |

### Próximos passos

1. Escolher um slice e executar via `execute-tdd-spec` (Fase 4).
2. Revalidar `dotnet build` e `dotnet test` após cada slice.
3. Ao concluir um Epic, executar QA e revisão (Fase 5).
