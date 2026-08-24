using Taskboard;
using Taskboard.ValueObjects;
using Shouldly;
using Xunit;

namespace Taskboard.Tests.Unit.Domain.ValueObjects;

public class TaskPriorityTests
{
    [Theory]
    [InlineData("none")]
    [InlineData("urgent")]
    [InlineData("high")]
    [InlineData("medium")]
    [InlineData("low")]
    public void Dado_ValorValido_Quando_Criar_Entao_RetornaPrioridade(string value)
    {
        var priority = TaskPriority.From(value);
        priority.Value.ShouldBe(value);
    }

    [Fact]
    public void Dado_ValorInvalido_Quando_Criar_Entao_LancaDomainException()
    {
        Should.Throw<DomainException>(() => TaskPriority.From("invalid"));
    }
}
