# SPEC-001: Visão Geral da Migração para C# .NET 10

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | Visão Geral da Migração para C# .NET 10 |
| Product / System | dashi-taskboard (Codex Taskboard) |
| Module / Bounded Context | Taskboard Platform |
| Change type | Migration |
| Repository | afonsoft/dashi-taskboard |
| Suggested branch | devin/spec-migration-net10 |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O repositório `afonsoft/dashi-taskboard` é uma aplicação local-first de gestão de tarefas (issue board) implementada em Node.js 22.5+, React 19, Vite, TypeScript e SQLite nativo (`node:sqlite`). A aplicação possui servidor HTTP raw `node:http`, CLI `taskctl`, servidor MCP, Skill para agentes de IA e interface web. O objetivo é produzir um clone funcional em C# .NET 10 / C# 14, preservando comportamento, contratos HTTP e integrações com agentes via MCP e Skill.

### Objective

Criar uma aplicação C# .NET 10 (ASP.NET Core minimal APIs + ABP N-Layer) que replique a funcionalidade atual: projetos, tarefas, comentários, anexos, relacionamentos, workspaces de workflow, chat de IA, sincronização Jira, CLI e servidor MCP.

### Expected outcome

Após a migração, a aplicação .NET 10 deve:
- Atender às mesmas rotas HTTP documentadas nos specs.
- Persistir os mesmos dados em SQLite/EF Core.
- Oferecer CLI equivalente ao `taskctl`.
- Expor servidor MCP com as mesmas ferramentas.
- Disponibilizar Skill para agentes no padrão Agent Skills.
- Reimplementar a UI em Blazor/MAUI.

### Out of scope

- Reescrita da UI web em outra tecnologia (escopo futuro; foco no backend + contratos).
- Empacotamento Tauri/MAUI para desktop nesta fase.
- Deploy Cloudflare Workers/D1/R2 (a ser tratado em SPEC-010).
- Modificação do repositório Node.js original.

---

## 2. Agent Role

The agent must act as:

> You are a senior software engineer specialized in C#, .NET 10, Clean Architecture, Domain-Driven Design, ABP N-Layer, automated testing, security, observability, and clean code.  
> Your responsibility is to implement the migration according to this SPEC without inventing requirements, expanding the scope, or making undocumented architectural decisions.

### Expected behavior

- Be conservative with architectural changes.
- Preserve backward compatibility whenever possible.
- Prioritize readability, testability, and maintainability.
- Make uncertainty explicit before implementing.
- Do not introduce external dependencies without justification.
- Do not remove existing tests without a documented technical reason.
- Do not change public contracts without declaring the impact.

---

## 3. Agent Autonomy Level

### Selected level

3

### Restrictions associated with this level

- Do not push directly to `main`.
- Do not change database schemas without migration and rollback plan.
- Do not publish packages or deploy automatically.
- Do not alter existing Node.js source code.

---

## 4. Product Context

### Functional context

O Taskboard é um quadro de tarefas local-first, compatível com múltiplos agentes de IA, com sincronização opcional Jira, CLI `taskctl` e servidor MCP. A migração para .NET 10 visa portar a camada de servidor, banco de dados, CLI, MCP e Skill.

### Technical context

Atual:
- Node.js 22.5+ com `node:sqlite`.
- Servidor raw `node:http` (não Fastify/Express).
- React 19 + Vite frontend.
- Tauri para desktop.
- CLI `taskctl` em `cli/taskctl.mjs`.
- MCP server em `mcp/index.mjs`.
- Skill em `skills/manage-taskboard/SKILL.md`.
- Cloud: Cloudflare/D1/R2 via `wrangler`.

### Relevant stack

- Language: C# 14
- Runtime: .NET 10
- Frameworks: ASP.NET Core Minimal APIs, ABP N-Layer, EF Core, MediatR, AutoMapper
- Tests: xUnit, Shouldly, NSubstitute
- Database: SQLite (local-first), opcional PostgreSQL/SQL Server
- Messaging: In-memory event bus (ABP), SSE para eventos em tempo real
- Observability: ILogger, OpenTelemetry, CorrelationId

### Relevant files or directories

```text
/server
  app.mjs
  database.mjs
  index.mjs
  cloud-config.mjs
/cli
  taskctl.mjs
/mcp
  index.mjs
/skills
  manage-taskboard/SKILL.md
/web
  src/components/*.tsx
/shared
  domain.mjs
```

### Context files the agent must read before implementation

- README.md
- AGENTS.md
- server/app.mjs
- server/database.mjs
- cli/taskctl.mjs
- mcp/index.mjs
- skills/manage-taskboard/SKILL.md
- shared/domain.mjs

---

## 5. Task Definition

### Main task

Migrar a aplicação `dashi-taskboard` do stack Node.js/React/SQLite para C# .NET 10, mantendo funcionalidade equivalente, contratos HTTP, CLI, MCP server e Skill.

### Subtasks

- Mapear domínio e entidades (SPEC-002).
- Mapear API REST e server HTTP (SPEC-004).
- Mapear CLI `taskctl` (SPEC-005).
- Mapear MCP server (SPEC-006).
- Mapear Skill de agente (SPEC-007).
- Mapear persistência e schema SQLite (SPEC-009).
- Mapear integrações Jira/Cloud (SPEC-010).
- Mapear IA / workflow workspaces (SPEC-011).
- Mapear Web UI (SPEC-008).
- Definir arquitetura, testes e plano de implantação (este SPEC).

### Do not do

- Não modificar o repositório Node.js original.
- Não implementar código C# nesta fase (somente specs).
- Não alterar frontend sem spec dedicada.

---

## 6. Functional Requirements

### FR-001: Manter funcionalidade de projetos

**Description:**  
O sistema deve permitir CRUD de projetos com `id`, `name`, `workspace_path`, `labels`, `next_task_number`, `created_at` e `updated_at`.

**Rules:**

- Identificador `local` é reservado para o projeto global (`全局`).
- `next_task_number` inicia em 1 e incrementa a cada tarefa criada.
- `labels` é uma lista JSON de strings.

**Inputs:**

| Field | Type | Required | Rule |
|---|---|---:|---|
| id | string | yes | máx 128 chars, único |
| name | string | yes | não vazio |
| workspacePath | string? | no | caminho absoluto ou null |
| labels | string[] | yes | default padrão do sistema |

**Outputs:**

| Field | Type | Description |
|---|---|---|
| project | ProjectDto | projeto criado/atualizado |

**Acceptance criteria:**

- [ ] GET /api/projects lista projetos com contagem de issues ativas.
- [ ] POST /api/projects cria projeto.
- [ ] Projeto `local` é inicializado automaticamente.

---

## 7. Business Rules

### BR-001: Projeto padrão local

O projeto `id='local'` com nome `全局` deve existir sempre que o banco de dados for inicializado. Ele serve como projeto padrão para tarefas locais sem workspace específico.

### BR-002: Identificadores únicos

Task identifiers (`TASK-{projectId}-{number}` ou `JIRA:{origin}:{key}`) devem ser únicos globalmente.

### BR-003: Labels por projeto

Cada projeto mantém sua própria lista de labels. Quando uma tarefa recebe novas labels, elas são adicionadas à lista do projeto.

### Domain invariants

- Um projeto não pode ser removido se houver tarefas ativas não-arquivadas.
- `next_task_number` nunca decrementa.

---

## 8. Domain Modeling

### Bounded Context

Taskboard Platform

### Aggregates

| Aggregate | Responsibility | Invariants |
|---|---|---|
| Project | Gerencia projetos, labels e numeração de tarefas | Número sequencial único |
| Task | Gerencia ciclo de vida de uma tarefa | Status, prioridade e versionamento |
| Comment | Comentários de uma tarefa | Pertence a uma tarefa |
| Attachment | Anexos de tarefas/comentários | Tamanho >= 0, kind válido |
| WorkflowWorkspace | Configuração de workflow visual por projeto | JSON válido |
| AiChatThread | Threads de conversação com agentes | status válido |

### Entities

| Entity | Identity | Responsibility |
|---|---|---|
| Project | ProjectId (string) | Agrupar tarefas e configurações |
| Task | TaskId (string) | Representar uma issue/tarefa |
| Comment | CommentId (string) | Registro textual em tarefa |
| Attachment | AttachmentId (string) | Arquivo anexado |
| TaskRelation | (relationType, sourceId, targetId) | Relacionamento entre tarefas |
| AiChatThread | ThreadId (string) | Conversa com agente |

### Value Objects

| Value Object | Fields | Validations |
|---|---|---|
| TaskStatus | Value | must be in statuses list |
| TaskPriority | Value | must be in priorities list |
| Actor | Type, Id, Name, AvatarUrl | type in ('user','agent') |
| Recurrence | Interval, Unit | unit in ('day','week','month','year') |

### Domain Events

| Event | When it occurs | Payload |
|---|---|---|
| TaskCreatedDomainEvent | Após criação de tarefa | TaskId, ProjectId |
| TaskMovedDomainEvent | Após alteração de status | TaskId, OldStatus, NewStatus |
| TaskUpdatedDomainEvent | Após patch | TaskId, Changes |
| TaskArchivedDomainEvent | Após arquivamento | TaskId |
| CommentAddedDomainEvent | Após novo comentário | TaskId, CommentId |

### Expected C# style

```csharp
public sealed class Project : AggregateRoot<ProjectId>
{
    private readonly List<string> _labels = new();
    public string Name { get; private set; }
    public string? WorkspacePath { get; private set; }
    public IReadOnlyCollection<string> Labels => _labels.AsReadOnly();
    public long NextTaskNumber { get; private set; } = 1;
    // factory, methods...
}
```

---

## 9. Expected Architecture

### Architectural style

ABP N-Layer / Clean Architecture / Modular Monolith

### Layers

```text
Domain
  .Domain.Shared
  .Domain
Application
  .Application.Contracts
  .Application
Infrastructure
  .EntityFrameworkCore
  .Mcp
  .Cli
  .HttpApi
Presentation/API
  .HttpApi.Host
  .Blazor (futuro)
Tests
  .Domain.Tests
  .Application.Tests
  .IntegrationTests
```

### Allowed dependencies

- `Domain` não depende de nenhuma outra camada.
- `Application.Contracts` define DTOs e interfaces.
- `Application` depende de `Domain`.
- `Infrastructure` implementa contratos do `Application`.
- `HttpApi.Host` orquestra entrada/saída.

### Forbidden dependencies

- Domain acessando banco, HTTP, filas, cache.
- Controller contendo regras de negócio.
- Handler contendo regras complexas de domínio.
- Repository validando regras de negócio.
- DTOs vazando para o domínio.

---

## 10. API Contracts

Ver especificações detalhadas em SPEC-004. Resumo:

```http
GET    /health
GET    /api/client-storage
GET    /api/local/codex-thread-progress
GET    /api/local/host-runtime
GET/PUT /api/local/cloud-session
GET/POST /api/local/jira-connection
POST   /api/local/jira-connection/sync
GET    /api/meta
GET/POST /api/local/ai/catalog
GET    /api/local/ai/composer/candidates
POST   /api/local/ai/composer/rebind
GET/POST /api/local/ai/threads
GET/PUT /api/device-workspaces
GET/PUT /api/workflow-capabilities
GET/POST /api/projects
GET/POST /api/tasks
GET    /api/events (SSE)
GET/POST/PATCH/DELETE /api/tasks/:id
GET/POST /api/tasks/:id/comments
GET/POST /api/attachments
GET/DELETE /api/attachments/:id/{content|download}
```

Error responses seguem ProblemDetails com códigos customizados: `INVALID_PATH`, `TASK_NOT_FOUND`, `VERSION_CONFLICT`, etc.

---

## 11. Application Contracts

### Commands/Queries principais

```csharp
public sealed record CreateProjectCommand(string Id, string Name, string? WorkspacePath) : IRequest<ProjectDto>;
public sealed record ListProjectsQuery() : IRequest<IReadOnlyList<ProjectDto>>;
public sealed record CreateTaskCommand(string ProjectId, string Title, string? Description, string Status, string Priority, IReadOnlyList<string> Labels) : IRequest<TaskDto>;
public sealed record UpdateTaskCommand(string TaskId, long Version, TaskPatch Changes, string? ThreadId, ThreadBinding? ThreadBinding, Actor Actor) : IRequest<TaskDto>;
public sealed record MoveTaskCommand(string TaskId, long Version, string Status, double? SortOrder, string? ThreadId, ThreadBinding? ThreadBinding, Actor Actor) : IRequest<TaskDto>;
```

Handlers encapsulam regras de domínio e chamam repositórios.

---

## 12. Persistence and Data

### Persisted entities

Ver SPEC-009 para schema detalhado. Entidades: `Projects`, `Tasks`, `Comments`, `TaskActivities`, `Attachments`, `WorkflowWorkspaces`, `ProjectSummaries`, `AiChatThreads`, `AiChatRuns`, `AiChatEvents`, `TaskRelations`.

### Migration required

Yes

### Migration strategy

- UP: criar tabelas e índices no schema inicial.
- DOWN: remover tabelas na ordem inversa (respeitando FK).

### Compatibility

- [x] Não quebra dados existentes (migração SQLite).
- [x] Inclui rollback.
- [x] Testes de migração no EF Core.
- [x] Não expõe dados sensíveis.

---

## 13. Integrations

### External services

| Service | Data sent | Data received | Security |
|---|---|---|---|
| Jira API | updates de issues | issues, comentários | OAuth/basic token armazenado em secrets |
| Cloudflare D1/R2 | dados sincronizados | dados remotos | Token de API Cloudflare |
| AI Models (OpenAI/Claude) | prompts, contexto | respostas, ações | API key, sandbox read-only/write |

### Expected failures

- Timeout Jira
- Jira indisponível
- Resposta inválida
- Conflito de versão

### Resilience strategy

- Timeouts explícitos.
- Retry para falhas transitórias.
- Circuit breaker para Jira.
- Fallback de sincronização manual.

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Projeto duplicado | POST /api/projects com id existente | 409 PROJECT_EXISTS |
| Tarefa não encontrada | GET /api/tasks/invalid | 404 TASK_NOT_FOUND |
| Conflito de versão | PATCH com version desatualizado | 409 VERSION_CONFLICT |
| Jira offline | sync forçado | 502 JIRA_RECONCILE_FAILED |
| Arquivo inexistente | GET /api/attachments/invalid | 404 ATTACHMENT_NOT_FOUND |

---

## 15. Few-Shot Examples

### Example 1: Criar projeto

**Input:**

```json
POST /api/projects
{
  "id": "my-project",
  "name": "My Project",
  "workspacePath": "/home/user/my-project"
}
```

**Expected output:**

```json
{
  "project": {
    "id": "my-project",
    "name": "My Project",
    "workspacePath": "/home/user/my-project",
    "labels": ["缺陷", "特性", "for-claude", "hold", "改进", "phase-1", "phase-2", "phase-3", "phase-4", "phase-5", "phase-6"],
    "issueCount": 0,
    "createdAt": "2026-08-24T01:53:00Z",
    "updatedAt": "2026-08-24T01:53:00Z"
  }
}
```

---

## 16. Non-Functional Requirements

### Performance

- P95 < 300ms para operações de leitura.
- Sync Jira < 30s para boards medianos.

### Security

- Não logar tokens, senhas ou dados pessoais.
- Validar autorização antes de acessar recursos.
- Sanitizar descrições e comentários (DOMPurify equivalente).

### Observability

- Structured logs com `ILogger`.
- Métricas de sucesso, erro e latência.
- Tracing para chamadas externas.
- CorrelationId no fluxo de requisições.

### Reliability

- Cancellation tokens respeitados.
- Timeouts para chamadas externas.
- Transações para persistência.
- Idempotência em comandos de sync.

---

## 17. Mandatory Guardrails

The agent must follow these rules:

- Do not invent requirements.
- Do not create a new architecture without justification.
- Do not modify a public contract without documenting the breaking change.
- Do not remove or ignore existing tests.
- Do not add a library without an explicit need.
- Do not place business rules in controllers.
- Do not access infrastructure directly from the domain layer.
- Do not expose secrets, tokens, personal data, or regulated data in logs.
- Do not deploy, push, or merge automatically.
- Do not modify CI/CD pipelines unless this SPEC has a dedicated section for it.
- Do not expand the scope with opportunistic improvements.
- Stop and request human review when there is critical ambiguity.

---

## 18. Expected Tests

### Unit tests

| Class | Scenarios |
|---|---|
| Project | criação, numeração, labels |
| Task | mudança de status, versionamento, prioridade |
| TaskRelation | parent, blocks, related |

### Integration tests

| Flow | Validation |
|---|---|
| POST /api/projects | 201 Created |
| GET /api/projects | retorna lista |
| POST /api/tasks | cria com identifier correto |
| PATCH /api/tasks/:id | version conflict 409 |

---

## 19. Acceptance Criteria

- [ ] Todos os specs de migração foram criados.
- [ ] Arquitetura .NET 10 ABP N-Layer foi definida.
- [ ] Contratos HTTP mapeados sem ambiguidade.
- [ ] Domínio e persistência mapeados.
- [ ] CLI, MCP e Skill mapeados.
- [ ] Plano de testes definido.
- [ ] Riscos e rollback documentados.

---

## 20. Implementation Plan

### Step 1: Discovery

- [x] Ler arquivos contexto.
- [x] Identificar arquitetura atual.
- [x] Extrair schema, rotas, CLI, MCP.

### Step 2: Technical design

- [ ] Definir classes de domínio.
- [ ] Definir commands/queries.
- [ ] Definir contratos API.
- [ ] Definir repositórios.
- [ ] Definir migrations.

### Step 3: Implementation

- [ ] Implementar domínio.
- [ ] Implementar aplicação.
- [ ] Implementar infraestrutura EF Core + SQLite.
- [ ] Implementar HttpApi.
- [ ] Implementar CLI.
- [ ] Implementar MCP server.
- [ ] Implementar Skill.

### Step 4: Tests

- [ ] Domain tests.
- [ ] Application tests.
- [ ] Integration tests.
- [ ] Contract tests.

### Step 5: Final validation

- [ ] Build.
- [ ] Tests.
- [ ] Review arquitetura.
- [ ] Documentação.

---

## 21. Rollback Strategy

### When to trigger rollback

- Erro 5xx aumentado.
- Quebra de contrato.
- Inconsistência de dados.

### How to revert

- Reverter branch.
- Restaurar backup do SQLite.
- Reduzir feature flag.

### Expected evidence

- Logs.
- Métricas.
- Smoke tests.

---

## 22. Risks and Mitigations

| Risk | Impact | Probability | Mitigation |
|---|---|---:|---|
| Diferença semântica entre `node:sqlite` e EF Core | Médio | Média | Mapear tipos SQL, testar migrations |
| MCP SDK .NET ainda em evolução | Médio | Baixa | Usar versão estável ou implementar server STDIO/SSE manual |
| Perda de funcionalidade de AI chat | Alto | Média | Especificar detalhadamente (SPEC-011) |
| CLI `taskctl` complexo | Médio | Média | Decompor em subcomandos (SPEC-005) |

---

## 23. Definition of Done

- [ ] SPEC revisado.
- [ ] Implementação segue o SPEC.
- [ ] Testes automatizados criados.
- [ ] Build validado.
- [ ] Contratos preservados ou versionados.
- [ ] Observabilidade implementada.
- [ ] Documentação atualizada.
- [ ] PR descreve mudanças, riscos e evidências.
- [ ] Nenhum TODO crítico no código.
- [ ] Nenhuma decisão arquitetural implícita.

---

## 24. Key Reminder

> The SPEC is the contract.  
> The agent must not optimize, expand, or reinterpret the scope.  
> In case of ambiguity, the agent must stop, make the uncertainty explicit, and propose technical options with impact, risk, and recommendation.

---

## Pending Questions and Ambiguities

1. O frontend será mantido em React/Vite servido estáticamente ou reescrito em Blazor?
2. Qual banco de dados deve ser o padrão em produção (SQLite local-first, PostgreSQL, SQL Server)?
3. Há necessidade de empacotamento desktop (MAUI/WinUI) nesta fase?
4. O servidor MCP deve usar STDIO, SSE ou HTTP?

## Human Approval Checklist

- [ ] O problema de negócio está claro.
- [ ] O resultado esperado é mensurável.
- [ ] O escopo e fora de escopo são explícitos.
- [ ] Requisitos funcionais são testáveis.
- [ ] Regras de negócio estão completas o suficiente.
- [ ] Modelo de domínio está alinhado ao bounded context.
- [ ] Contratos API são explícitos.
- [ ] Casos de borda listados.
- [ ] Requisitos não-funcionais definidos.
- [ ] Guardrails claros.
- [ ] Testes necessários claros.
- [ ] Estratégia de rollback definida.
- [ ] Riscos e mitigações documentados.
- [ ] Ambiguidades resolvidas ou aceitas.
