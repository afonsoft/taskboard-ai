using System.Diagnostics;
using System.Runtime.InteropServices;
using Taskboard.Agents;

namespace Taskboard.Integrations.Agents;

/// <summary>
/// Descobre agentes CLI instalados no servidor a partir do PATH.
/// </summary>
public sealed class AgentDiscoveryService : IAgentDiscoveryService
{
    private static readonly Dictionary<AgentType, string> KnownAgents = new()
    {
        [AgentType.Devin] = "devin",
        [AgentType.Claude] = "claude",
        [AgentType.Codex] = "codex",
        [AgentType.OpenCode] = "opencode",
        [AgentType.OpenHands] = "openhands"
    };

    public Task<IReadOnlyList<AgentInfo>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        var agents = new List<AgentInfo>();

        foreach (var (type, name) in KnownAgents)
        {
            var executablePath = PathSearch.FindExecutable(name);
            if (executablePath is null)
            {
                agents.Add(new AgentInfo(name, string.Empty, type, AgentStatus.Unavailable, null));
                continue;
            }

            var version = TryGetVersion(executablePath, cancellationToken);
            agents.Add(new AgentInfo(name, executablePath, type, AgentStatus.Available, version));
        }

        return Task.FromResult<IReadOnlyList<AgentInfo>>(agents.AsReadOnly());
    }

    public string? ResolveExecutablePath(AgentType agentType)
    {
        if (!KnownAgents.TryGetValue(agentType, out var name))
        {
            return null;
        }

        return PathSearch.FindExecutable(name);
    }

    private static string? TryGetVersion(string executablePath, CancellationToken cancellationToken)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(executablePath, "--version")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process is null)
            {
                return null;
            }

            process.WaitForExit(2000);
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Ignora falhas ao encerrar processo de versão.
                }

                return null;
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            if (!string.IsNullOrWhiteSpace(output))
            {
                var firstLine = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                return firstLine;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
