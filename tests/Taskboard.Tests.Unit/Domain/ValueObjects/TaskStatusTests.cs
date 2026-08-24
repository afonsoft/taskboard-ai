using Taskboard;
using TaskStatus = Taskboard.ValueObjects.TaskStatus;
using Shouldly;
using Xunit;

namespace Taskboard.Tests.Unit.Domain.ValueObjects;

public class TaskStatusTests
{
    [Theory]
    [InlineData("backlog")]
    [InlineData("todo")]
    [InlineData("in_progress")]
    [InlineData("in_review")]
    [InlineData("blocked")]
    [InlineData("done")]
    [InlineData("canceled")]
    public void Dado_ValorValido_Quando_Criar_Entao_RetornaStatus(string value)
    {
        var status = TaskStatus.From(value);
        status.Value.ShouldBe(value);
    }

    [Fact]
    public void Dado_ValorInvalido_Quando_Criar_Entao_LancaDomainException()
    {
        Should.Throw<DomainException>(() => TaskStatus.From("invalid"));
    }
}
