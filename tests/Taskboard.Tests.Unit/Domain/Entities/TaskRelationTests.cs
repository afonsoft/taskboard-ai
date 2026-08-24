using Taskboard;
using Taskboard.Domain.Entities;
using Taskboard.ValueObjects;
using Shouldly;
using Xunit;

namespace Taskboard.Tests.Unit.Domain.Entities;

public class TaskRelationTests
{
    private static readonly DateTime Now = new(2026, 8, 24, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Dado_TarefasDiferentes_Quando_CriarParent_Entao_RelacaoCriada()
    {
        var source = TaskId.From("source-1");
        var target = TaskId.From("target-1");

        var relation = TaskRelation.Create(source, target, RelationType.Parent, Now);

        relation.SourceTaskId.ShouldBe(source);
        relation.TargetTaskId.ShouldBe(target);
        relation.RelationType.ShouldBe(RelationType.Parent);
    }

    [Fact]
    public void Dado_AutoRelacionamento_Quando_Criar_Entao_LancaDomainException()
    {
        var taskId = TaskId.From("same");

        Should.Throw<DomainException>(() => TaskRelation.Create(taskId, taskId, RelationType.Related, Now));
    }

    [Fact]
    public void Dado_RelatedInvertido_Quando_VerificarEquivalencia_Entao_Simetrico()
    {
        var a = TaskId.From("a");
        var b = TaskId.From("b");

        var relation = TaskRelation.Create(a, b, RelationType.Related, Now);

        relation.IsEquivalent(a, b, RelationType.Related).ShouldBeTrue();
        relation.IsEquivalent(b, a, RelationType.Related).ShouldBeTrue();
    }

    [Fact]
    public void Dado_Related_Quando_ConsultarIsEquivalentComInverso_Entao_Symmetric()
    {
        var a = TaskId.From("a");
        var b = TaskId.From("b");

        var relationAB = TaskRelation.Create(a, b, RelationType.Related, Now);
        var relationBA = TaskRelation.Create(b, a, RelationType.Related, Now);

        relationAB.IsEquivalent(a, b, RelationType.Related).ShouldBeTrue();
        relationAB.IsEquivalent(b, a, RelationType.Related).ShouldBeTrue();

        relationBA.IsEquivalent(a, b, RelationType.Related).ShouldBeTrue();
        relationBA.IsEquivalent(b, a, RelationType.Related).ShouldBeTrue();
    }

    [Fact]
    public void Dado_Blocks_Quando_Inverter_Entao_NaoEquivalente()
    {
        var a = TaskId.From("a");
        var b = TaskId.From("b");

        var relationAB = TaskRelation.Create(a, b, RelationType.Blocks, Now);

        relationAB.IsEquivalent(a, b, RelationType.Blocks).ShouldBeTrue();
        relationAB.IsEquivalent(b, a, RelationType.Blocks).ShouldBeFalse();
    }

    [Fact]
    public void Dado_Parent_Quando_Inverter_Entao_NaoEquivalente()
    {
        var a = TaskId.From("a");
        var b = TaskId.From("b");

        var relationAB = TaskRelation.Create(a, b, RelationType.Parent, Now);

        relationAB.IsEquivalent(a, b, RelationType.Parent).ShouldBeTrue();
        relationAB.IsEquivalent(b, a, RelationType.Parent).ShouldBeFalse();
    }
}
