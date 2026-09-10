namespace Taskboard.Agents;

/// <summary>
/// Métodos auxiliares para converter <see cref="AgentType"/> em labels e nomes amigáveis.
/// </summary>
public static class AgentTypeExtensions
{
    /// <summary>
    /// Converte o tipo de agente em um nome de label compatível com o GitHub.
    /// </summary>
    public static string ToLabel(this AgentType agentType) => agentType switch
    {
        AgentType.Devin => "agent-devin",
        AgentType.Claude => "agent-claude",
        AgentType.Codex => "agent-codex",
        AgentType.OpenCode => "agent-opencode",
        AgentType.OpenHands => "agent-openhands",
        _ => $"agent-{agentType.ToString().ToLowerInvariant()}"
    };
}
