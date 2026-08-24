using Taskboard;
using Taskboard.Domain.Entities;
using Taskboard.Domain.Events;
using Taskboard.ValueObjects;
using Shouldly;
using Xunit;

namespace Taskboard.Tests.Unit.Domain.Entities;

public class ProjectTests
{
    private static readonly DateTime Now = new(2026, 8, 24, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Dado_DadosValidos_Quando_Criar_Entao_PropriedadesEstaoCorretas()
    {
        var project = Project.Create(ProjectId.From("my"), "My Project", "/workspace", Now);

        project.Name.ShouldBe("My Project");
        project.WorkspacePath.ShouldBe("/workspace");
        project.NextTaskNumber.ShouldBe(1);
        project.Labels.ShouldBeEmpty();
    }

    [Fact]
    public void Dado_ProjetoLocal_Quando_Criar_Entao_IdLocalENomeGlobal()
    {
        var project = Project.Local(Now);

        project.Id.Value.ShouldBe("local");
        project.Name.ShouldBe("全局");
    }

    [Fact]
    public void Dado_ProximoNumero_Quando_GerarIdentifier_Entao_FormatoCorretoEIncrementa()
    {
        var project = Project.Create(ProjectId.From("my"), "My Project", null, Now);

        var identifier = project.GenerateTaskIdentifier();

        identifier.Value.ShouldBe("TASK-my-1");
        project.NextTaskNumber.ShouldBe(2);
        project.Version.ShouldBe(2);
    }

    [Fact]
    public void Dado_LabelNovo_Quando_Adicionar_Entao_LabelExisteEDomaiEventGerado()
    {
        var project = Project.Create(ProjectId.From("my"), "My Project", null, Now);

        project.AddLabel("feature", Now);

        project.Labels.ShouldContain("feature");
        project.DomainEvents.ShouldContain(e => e.GetType() == typeof(ProjectLabelsUpdatedDomainEvent));
    }

    [Fact]
    public void Dado_NomeVazio_Quando_Criar_Entao_LancaDomainException()
    {
        Should.Throw<DomainException>(() => Project.Create(ProjectId.From("my"), "", null, Now));
    }
}
