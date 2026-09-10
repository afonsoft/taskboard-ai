namespace Taskboard.Agents;

/// <summary>
/// Broadcast de mensagens de log dos agentes para os clientes conectados.
/// </summary>
public interface IAgentLogBroadcaster
{
    /// <summary>
    /// Envia a mensagem de log para os listeners da issue.
    /// </summary>
    Task BroadcastAsync(AgentLogMessage message, CancellationToken cancellationToken = default);
}
