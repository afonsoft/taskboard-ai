using System;
using Taskboard.Agents;
using Taskboard.Domain.Entities;
using Shouldly;
using Xunit;

namespace Taskboard.Tests.Unit.Domain.Entities;

public class AgentPreferenceTests
{
    [Fact]
    public void Dado_UmNovoAgente_Quando_Criar_Entao_HabilitadoPorPadrao()
    {
        var preference = new AgentPreference(Guid.NewGuid(), AgentType.Claude);

        preference.AgentType.ShouldBe(AgentType.Claude);
        preference.Enabled.ShouldBe(true);
    }
}
