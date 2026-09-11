using NSubstitute;
using Shouldly;
using System.Collections.Generic;
using Taskboard.Agents;
using Taskboard.Integrations.Agents;
using Xunit;

namespace Taskboard.Tests.Unit.Agents;

public class JsonRpcAcpClientTests
{
    [Fact]
    public async Task Dado_RespostaJsonRpc_Quando_Executar_Entao_RetornaSucessoComExitCodeZero()
    {
        // Covers FR-002: Transporte bidirecional
        var adapter = Substitute.For<IAgentAdapter>();
        adapter.CanHandle(Arg.Any<AgentType>()).Returns(true);
        adapter.BuildCommand(Arg.Any<AgentExecutionRequest>())
            .Returns(new AgentCommand("/bin/sh", ["-c", "read -r line; echo '{\"jsonrpc\":\"2.0\",\"id\":\"issue-1\",\"result\":{\"exitCode\":0}}'"], "/"));

        var client = new JsonRpcAcpClient([adapter]);
        var request = new AgentExecutionRequest(
            "issue-1",
            1,
            "owner/repo",
            "/workspace",
            null,
            null,
            "Implement.",
            AgentType.Codex);

        var result = await client.ExecuteAsync(request, null!);

        result.IsSuccess.ShouldBeTrue();
        result.ExitCode.ShouldBe(0);
    }
}
