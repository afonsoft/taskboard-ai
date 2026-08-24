using Taskboard;
using Taskboard.ValueObjects;
using Shouldly;
using Xunit;

namespace Taskboard.Tests.Unit.Domain.ValueObjects;

public class TaskIdentifierTests
{
    [Fact]
    public void Dado_ProjetoENumero_Quando_CriarLocal_Entao_FormatoCorreto()
    {
        var projectId = ProjectId.From("my-project");
        var identifier = TaskIdentifier.ForLocalTask(projectId, 1);
        identifier.Value.ShouldBe("TASK-my-project-1");
    }

    [Fact]
    public void Dado_OriginEExternalKey_Quando_CriarJira_Entao_FormatoCorreto()
    {
        var identifier = TaskIdentifier.ForJira("acme", "PROJ-42");
        identifier.Value.ShouldBe("JIRA:acme:PROJ-42");
    }

    [Fact]
    public void Dado_ValorInvalido_Quando_Criar_Entao_LancaDomainException()
    {
        Should.Throw<DomainException>(() => new TaskIdentifier("BAD-123"));
    }
}
