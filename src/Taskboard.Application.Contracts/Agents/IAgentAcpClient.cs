namespace Taskboard.Agents;

/// <summary>
/// Cliente ACP que executa o processo do agente e devolve os logs em tempo real.
/// </summary>
public interface IAgentAcpClient
{
    /// <summary>
    /// Envia a requisição para o agente, captura stdout/stderr e retorna o resultado final.
    /// </summary>
    Task<AgentExecutionResult> ExecuteAsync(
        AgentExecutionRequest request,
        IProgress<AgentLogMessage> progress,
        CancellationToken cancellationToken = default);
}
