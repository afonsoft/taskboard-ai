using System;
using System.Collections.Generic;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using NSubstitute;
using Taskboard.Agents;
using Taskboard.Application.Contracts.Settings;
using Taskboard.Application.Settings;
using Taskboard.Domain.Entities;
using Taskboard.Repositories;
using Shouldly;
using Xunit;

namespace Taskboard.Tests.Unit.Application.Settings;

public class SettingsServiceTests
{
    [Fact]
    public async Task Dado_PreferenciasExistentes_Quando_Obter_Entao_RetornaTemaEAgentes()
    {
        var userRepo = Substitute.For<IRepository<UserPreference>>();
        userRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UserPreference>>(new List<UserPreference>
            {
                new UserPreference(Guid.Empty) { Theme = "light", GitHubToken = "ghp_***" }
            }));

        var agentRepo = Substitute.For<IRepository<AgentPreference>>();
        agentRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AgentPreference>>(new List<AgentPreference>
            {
                new AgentPreference(Guid.NewGuid(), AgentType.Claude) { Enabled = false }
            }));

        var discovery = Substitute.For<IAgentDiscoveryService>();
        discovery.DiscoverAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AgentInfo>>(new List<AgentInfo>
            {
                new AgentInfo("claude", "/usr/bin/claude", AgentType.Claude, AgentStatus.Available, "1.0")
            }));

        var service = new SettingsService(userRepo, agentRepo, discovery);

        var settings = await service.GetSettingsAsync();

        settings.Theme.ShouldBe("light");
        settings.GitHubToken.ShouldBe("ghp_***");
        settings.Agents.Count.ShouldBe(1);
        settings.Agents[0].Type.ShouldBe(AgentType.Claude);
        settings.Agents[0].Enabled.ShouldBe(false);
    }
}
