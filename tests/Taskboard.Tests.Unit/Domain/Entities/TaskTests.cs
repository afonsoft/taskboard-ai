using Taskboard;
using Taskboard.Domain.Entities;
using Taskboard.Domain.Events;
using TaskStatus = Taskboard.ValueObjects.TaskStatus;
using Taskboard.ValueObjects;
using Shouldly;
using Xunit;

namespace Taskboard.Tests.Unit.Domain.Entities;

public class TaskTests
{
    private static readonly DateTime Now = new(2026, 8, 24, 0, 0, 0, DateTimeKind.Utc);

    private static Project CreateProject()
        => Project.Create(ProjectId.From("my"), "My Project", null, Now);

    private static Taskboard.Domain.Entities.Task CreateTask()
    {
        var project = CreateProject();
        var identifier = project.GenerateTaskIdentifier();
        return Taskboard.Domain.Entities.Task.Create(
            TaskId.NewGuid(),
            identifier,
            project.Id,
            "Nova feature",
            null,
            TaskStatus.Todo,
            TaskPriority.High,
            Actor.LocalUser(),
            now: Now);
    }

    [Fact]
    public void Dado_DadosValidos_Quando_CriarTarefa_Entao_PropriedadesEEventoCorretos()
    {
        var task = CreateTask();

        task.Title.ShouldBe("Nova feature");
        task.Status.ShouldBe(TaskStatus.Todo);
        task.Priority.ShouldBe(TaskPriority.High);
        task.Version.ShouldBe(1);
        task.DomainEvents.ShouldContain(e => e.GetType() == typeof(TaskCreatedDomainEvent));
    }

    [Fact]
    public void Dado_TarefaAtiva_Quando_Mover_Entao_StatusEOrdemAtualizados()
    {
        var task = CreateTask();
        task.ClearDomainEvents();

        task.Move(TaskStatus.InProgress, 1.5d, Actor.LocalUser(), Now);

        task.Status.ShouldBe(TaskStatus.InProgress);
        task.SortOrder.ShouldBe(1.5d);
        task.Version.ShouldBe(2);
        task.DomainEvents.ShouldContain(e => e.GetType() == typeof(TaskMovedDomainEvent) && ((TaskMovedDomainEvent)e).NewStatus == "in_progress");
    }

    [Fact]
    public void Dado_TarefaArquivada_Quando_Mover_Entao_LancaDomainException()
    {
        var task = CreateTask();
        task.Archive(Actor.LocalUser(), Now);

        Should.Throw<DomainException>(() => task.Move(TaskStatus.InProgress, null, Actor.LocalUser(), Now));
    }

    [Fact]
    public void Dado_TarefaAtiva_Quando_Arquivar_Entao_ArchivedAtPreenchidoEVersionIncrementa()
    {
        var task = CreateTask();
        task.ClearDomainEvents();

        task.Archive(Actor.LocalUser(), Now);

        task.ArchivedAt.ShouldBe(Now);
        task.Version.ShouldBe(2);
        task.DomainEvents.ShouldContain(e => e.GetType() == typeof(TaskArchivedDomainEvent));
    }

    [Fact]
    public void Dado_TarefaArquivada_Quando_Restaurar_Entao_ArchivedAtLimpo()
    {
        var task = CreateTask();
        task.Archive(Actor.LocalUser(), Now);
        task.ClearDomainEvents();

        task.Restore(Actor.LocalUser(), Now);

        task.ArchivedAt.ShouldBeNull();
        task.Version.ShouldBe(3);
        task.DomainEvents.ShouldContain(e => e.GetType() == typeof(TaskRestoredDomainEvent));
    }

    [Fact]
    public void Dado_TarefaJira_Quando_Arquivar_Entao_LancaDomainException()
    {
        var project = CreateProject();
        var identifier = project.GenerateTaskIdentifier();
        var task = Taskboard.Domain.Entities.Task.Create(
            TaskId.NewGuid(),
            identifier,
            project.Id,
            "Jira issue",
            null,
            TaskStatus.Todo,
            TaskPriority.Medium,
            Actor.LocalUser(),
            now: Now,
            externalSource: "jira",
            externalOrigin: "acme",
            externalId: "123",
            externalKey: "PROJ-1",
            externalUrl: "https://jira/PROJ-1");

        Should.Throw<DomainException>(() => task.Archive(Actor.LocalUser(), Now));
    }

    [Fact]
    public void Dado_VersaoCorreta_Quando_AplicarPatch_Entao_CamposAtualizados()
    {
        var task = CreateTask();
        task.ClearDomainEvents();

        task.ApplyPatch(
            new TaskPatch(Title: "Outro título", Priority: "low"),
            task.Version,
            Now);

        task.Title.ShouldBe("Outro título");
        task.Priority.ShouldBe(TaskPriority.Low);
        task.Version.ShouldBe(2);
        task.DomainEvents.ShouldContain(e => e.GetType() == typeof(TaskUpdatedDomainEvent));
    }

    [Fact]
    public void Dado_VersaoIncorreta_Quando_AplicarPatch_Entao_LancaDomainException()
    {
        var task = CreateTask();

        Should.Throw<DomainException>(() =>
            task.ApplyPatch(new TaskPatch(Title: "X"), task.Version + 1, Now));
    }

    [Fact]
    public void Dado_TituloVazio_Quando_Criar_Entao_LancaDomainException()
    {
        Should.Throw<DomainException>(() =>
            Taskboard.Domain.Entities.Task.Create(
                TaskId.NewGuid(),
                TaskIdentifier.ForLocalTask(ProjectId.From("p"), 1),
                ProjectId.From("p"),
                "",
                null,
                TaskStatus.Todo,
                TaskPriority.None,
                Actor.LocalUser(),
                now: Now));
    }
}
