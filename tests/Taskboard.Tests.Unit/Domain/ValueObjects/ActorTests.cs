using Taskboard;
using Taskboard.ValueObjects;
using Shouldly;
using Xunit;

namespace Taskboard.Tests.Unit.Domain.ValueObjects;

public class ActorTests
{
    [Fact]
    public void Dado_DadosValidos_Quando_CriarUsuario_Entao_RetornaActor()
    {
        var actor = Actor.User("u1", "Alice");
        actor.Type.ShouldBe("user");
        actor.Id.ShouldBe("u1");
        actor.Name.ShouldBe("Alice");
    }

    [Fact]
    public void Dado_TipoInvalido_Quando_Criar_Entao_LancaDomainException()
    {
        Should.Throw<DomainException>(() => new Actor("bot", "b1", "Bot"));
    }

    [Fact]
    public void Dado_LocalUser_Quando_Criar_Entao_RetornaUsuarioLocal()
    {
        var actor = Actor.LocalUser();
        actor.Type.ShouldBe("user");
        actor.Id.ShouldBe("local");
    }
}
