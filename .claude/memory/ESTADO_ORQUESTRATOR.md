# ESTADO_ORCHESTRATOR

> Arquivo de estado da skill `orchestrator` para o repositório `taskboard-ai`. Ler ao iniciar a sessão; escrever ao final de cada fase.

---

## Sessão

- **iniciado_em**: `2026-09-10 13:40:00`
- **fase_atual**: `Fase 4`
- **repositorio**: `afonsoft/taskboard-ai`
- **branch_trabalho**: `feature/agent-orchestration`
- **framework**: `afonsoft/skills` instalado via `npx skills add afonsoft/skills` (ver `skills-lock.json`)

---

## Fase 0 — Preconditions

| Item | Status |
|---|---|
| Git inicializado | ✅ |
| Remote `origin` GitHub | ✅ `afonsoft/taskboard-ai` |
| Acesso ao repositório | ✅ |

---

## Fase 2 — Audit do Harness

| Item | Status |
|---|---|
| `CLAUDE.md` (fonte única) | ✅ |
| `.claude/settings.json` | ✅ |
| `.claude/rules/global-rules.md` | ✅ |
| `.claude/agents/` (review, plan, test) | ✅ |
| `.claude/CONTEXT.md`, `RULES.md`, `TOOLS.md`, `WORKFLOWS.md`, `README.md`, `MEMORY.md` | ✅ |
| `.claude/memory/` | ✅ (criado nesta sessão) |
| `.specs/` | ✅ SPEC-000..015 |
| `docs/architecture/` | ✅ |
| Skills instaladas (`.claude/skills`, `.devin/skills`) | ✅ 21 skills afonsoft + `taskboard`, `testing-taskboard` |
| `.devin/config.json` | ✅ |

---

## GAPs Identificados

| # | ID | Dimensão | Severidade | Descritivo | Tier Risco | Status |
|---|----|----------|------------|------------|------------|--------|
| 1 | `GAP-001` | CI/CD | P2 | Workflow `dotnet.yml` sem cache NuGet, sem `concurrency`, sem `permissions` mínimas, sem SonarCloud, sem CodeQL/Dependabot | T2 Batchável (workflows protegidos — requer aprovação humana) | 🟡 queued |
| 2 | `GAP-002` | Documentação | P4 | Docs não cobriam Kanban GitHub nem orquestração de agentes | T1 Auto | 🟢 done |
| 3 | `GAP-003` | Arquitetura | P3 | Logs de agentes apenas em memória (perdidos em restart) | T2 Batchável | 🟡 queued |
| 4 | `GAP-004` | Arquitetura | P3 | Transporte ACP atual é stdin/stdout de linha; JSON-RPC não implementado | T2 Batchável | 🟡 queued |

---

## Tarefas (Fase 4 — Fila DAG)

### Tarefas Pendentes

```yaml
- id: TASK-003
  desc: "Melhorar GitHub Actions (cache, concurrency, permissions, SonarCloud, CodeQL, Dependabot)"
  skill: /dotnet-github-actions
  gap_ref: GAP-001
  issue_ref: ""
  spec_ref: ""
  depends_on: []
  status: pending_approval

- id: TASK-004
  desc: "Persistir AgentLogMessage em SQLite (EF Core)"
  skill: /tdd-spec
  gap_ref: GAP-003
  issue_ref: ""
  spec_ref: ".specs/SPEC-015-agent-orchestration.md"
  depends_on: []
  status: ready

- id: TASK-005
  desc: "Adapter ACP JSON-RPC (stdin/stdout) para agentes que suportam o protocolo"
  skill: /tdd-spec
  gap_ref: GAP-004
  issue_ref: ""
  spec_ref: ".specs/SPEC-015-agent-orchestration.md"
  depends_on: []
  status: ready
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
  status: in_progress

- id: TASK-007
  desc: "Implementar instalador CLI `install-cli.sh` em /usr/local/bin (SPEC-20260910-install-cli-sh)"
  skill: /tdd-spec
  gap_ref: GAP-006
  issue_ref: "https://github.com/afonsoft/taskboard-ai/issues/25"
  spec_ref: ".specs/SPEC-20260910-install-cli-sh.md"
  depends_on: []
  status: ready
```

### Próximos passos

1. Criar Issues no GitHub para `TASK-006` e `TASK-007` (Phase 3).
2. Iniciar execução com `/tdd-spec` na ordem de prioridade escolhida.
3. Re-validar após cada slice.
