using Taskboard.Domain.Entities;
using Shouldly;
using Xunit;

namespace Taskboard.Tests.Unit.Domain.Entities;

public class UserPreferenceTests
{
    [Fact]
    public void Dado_UmaNovaPreferencia_Quando_Criar_Entao_TemaPadraoEDark()
    {
        var preference = new UserPreference(Guid.NewGuid());

        preference.Theme.ShouldBe("dark");
        preference.GitHubToken.ShouldBeNull();
    }
}
