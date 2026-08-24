using Taskboard;
using Taskboard.ValueObjects;
using Shouldly;
using Xunit;

namespace Taskboard.Tests.Unit.Domain.ValueObjects;

public class RecurrenceTests
{
    [Theory]
    [InlineData(1, "day")]
    [InlineData(2, "week")]
    [InlineData(3, "month")]
    [InlineData(4, "year")]
    public void Dado_ValorValido_Quando_Criar_Entao_RetornaRecorrencia(int interval, string unit)
    {
        var recurrence = new Recurrence(interval, unit);
        recurrence.Interval.ShouldBe(interval);
        recurrence.Unit.ShouldBe(unit);
    }

    [Fact]
    public void Dado_IntervaloInvalido_Quando_Criar_Entao_LancaDomainException()
    {
        Should.Throw<DomainException>(() => new Recurrence(0, "day"));
    }

    [Fact]
    public void Dado_UnidadeInvalida_Quando_Criar_Entao_LancaDomainException()
    {
        Should.Throw<DomainException>(() => new Recurrence(1, "hour"));
    }
}
