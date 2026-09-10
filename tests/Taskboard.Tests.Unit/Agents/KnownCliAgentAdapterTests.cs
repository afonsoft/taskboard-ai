using System.Runtime.InteropServices;
using Shouldly;
using Taskboard.Agents;
using Taskboard.Integrations.Agents;
using Xunit;

namespace Taskboard.Tests.Unit.Agents;

[Collection("PathEnvironment")]
public class KnownCliAgentAdapterTests
{
    [Theory]
    [InlineData(AgentType.Devin)]
    [InlineData(AgentType.Claude)]
    [InlineData(AgentType.Codex)]
    [InlineData(AgentType.OpenCode)]
    [InlineData(AgentType.OpenHands)]
    public void Dado_TipoDeAgenteConhecido_Quando_VerificarSuporte_Entao_RetornaVerdadeiro(AgentType agentType)
    {
        var adapter = new KnownCliAgentAdapter();

        adapter.CanHandle(agentType).ShouldBeTrue();
    }

    [Fact]
    public void Dado_CodexDisponivelNoPath_Quando_MontarComando_Entao_IncluiPromptERepositorio()
    {
        var directory = Directory.CreateTempSubdirectory("taskboard-agent-tests-");
        var executablePath = Path.Combine(directory.FullName, "codex");
        File.WriteAllText(executablePath, "#!/bin/sh\n");
        SetExecutable(executablePath);

        var previousPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", directory.FullName);
            var request = CriarRequest(AgentType.Codex);

            var command = new KnownCliAgentAdapter().BuildCommand(request);

            command.ExecutablePath.ShouldBe(executablePath);
            command.WorkingDirectory.ShouldBe(request.RepoPath);
            command.Arguments.Count.ShouldBe(1);
            command.Arguments[0].ShouldContain($"Branch: {request.Branch}");
            command.Arguments[0].ShouldContain($"Scope: {request.Scope}");
            command.Arguments[0].ShouldContain(request.Instructions);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", previousPath);
            Directory.Delete(directory.FullName, recursive: true);
        }
    }

    [Fact]
    public void Dado_ExecutavelAusenteNoPath_Quando_MontarComando_Entao_LancaFileNotFoundException()
    {
        var directory = Directory.CreateTempSubdirectory("taskboard-agent-tests-");
        var previousPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", directory.FullName);

            Should.Throw<FileNotFoundException>(() =>
                new KnownCliAgentAdapter().BuildCommand(CriarRequest(AgentType.Codex)));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", previousPath);
            Directory.Delete(directory.FullName, recursive: true);
        }
    }

    private static AgentExecutionRequest CriarRequest(AgentType agentType)
        => new(
            "issue-1",
            1,
            "owner/repo",
            "/workspace/repo",
            "feature/agents",
            "src/Agents",
            "Implementar a orquestração.",
            agentType);

    private static void SetExecutable(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        File.SetUnixFileMode(
            path,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute);
    }
}
