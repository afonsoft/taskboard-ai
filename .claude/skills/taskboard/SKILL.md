---
name: taskboard
description: >
  Implement, review and document the taskboard-ai .NET 10 system. Follow the
  merged specs in .specs/, ABP N-Layer DDD, C# 14 conventions, xUnit + Shouldly
  + NSubstitute tests, and the global rules. Triggers on C#/.NET work, spec
  updates, CLI/MCP/REST/SSE features, and agent skill maintenance.
tools:
  - Read
  - Edit
  - Write
  - Grep
  - Glob
  - Bash
  - Mcp
---

## Contexto

O `taskboard-ai` é um clone local-first do `dashi-taskboard` em C# 14 / .NET 10. As specs unificadas em `.specs/` definem o contrato. A arquitetura é ABP N-Layer com DDD: Domain → Application.Contracts → Application → EntityFrameworkCore → Server.

## Atuação

1. Antes de implementar, leia `CLAUDE.md`, `.claude/rules/global-rules.md` e as specs relevantes em `.specs/`.
2. Planeje usando `.claude/agents/plan.md` para tarefas complexas.
3. Implemente com o menor escopo possível (minimal changes).
4. Valide com `dotnet build` e `dotnet test`.
5. Atualize `.specs/` e `docs/` se contratos/arquitetura mudarem.

## Restrições

- Não commitar/push em `main`, `master`, `develop`.
- Não modificar `/.github/workflows` sem aprovação.
- Nunca expor secrets (tokens, senhas, API keys).
- Não implementar sem `Execution Plan` para tarefas multi-arquivo.

## Convenções Específicas

- **C# 14 / .NET 10 / ABP N-Layer DDD**
- **xUnit + Shouldly + NSubstitute** (não Moq)
- **Testes BDD em português**: `Dado_UmaTarefa_Quando_AtualizarStatus_Entao_DeveRetornarOk`
- **Optimistic concurrency**: `long Version` + `VERSION_CONFLICT` 409
- **Minimal APIs** para REST/SSE
- **System.CommandLine** para CLI `taskctl`
- **ModelContextProtocol SDK .NET** para MCP server

## Exemplos

### Criar um command handler

```csharp
public sealed record CreateTaskCommand(
    ProjectId ProjectId,
    string Title,
    Status Status,
    Priority Priority
) : IRequest<TaskDto>;

internal sealed class CreateTaskCommandHandler(ITaskRepository tasks, IUnitOfWork uow)
    : IRequestHandler<CreateTaskCommand, TaskDto>
{
    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        var task = Task.Create(request.ProjectId, request.Title, request.Status, request.Priority);
        await tasks.InsertAsync(task, ct);
        await uow.SaveChangesAsync(ct);
        return task.ToDto();
    }
}
```

### Endpoint Minimal API

```csharp
app.MapPost("/api/projects/{projectId}/tasks", async (
    [FromRoute] string projectId,
    [FromBody] CreateTaskCommand command,
    [FromServices] IMediator mediator,
    CancellationToken ct) =>
{
    var cmd = command with { ProjectId = new ProjectId(projectId) };
    var dto = await mediator.Send(cmd, ct);
    return Results.Created($"/api/projects/{projectId}/tasks/{dto.Id}", dto);
});
```

### Teste de domínio

```csharp
[Fact(DisplayName = "Dado uma tarefa nova quando atualizar status então deve registrar evento")]
public void Dado_TarefaNova_Quando_AtualizarStatus_Entao_DeveRegistrarEvento()
{
    var task = Task.Create(new ProjectId("local"), "Título", Status.Todo, Priority.Medium);
    task.UpdateStatus(Status.InProgress);
    task.Status.ShouldBe(Status.InProgress);
    task.DomainEvents.ShouldContain(e => e is TaskStatusChangedEvent);
}
```

## Referências

- `.specs/CAPABILITY-MAP.md` — ordem de build
- `.specs/SPEC-000-overview.md`
- `.claude/agents/plan.md`
- `.claude/agents/review.md`
- `.claude/agents/test.md`
- `docs/`
