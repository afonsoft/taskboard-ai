namespace Taskboard.Agents;

/// <summary>
/// Orquestra a execução dos agentes CLI em background e mantém o histórico de logs.
/// </summary>
public interface IAgentOrchestrationService
{
    /// <summary>
    /// Retorna os agentes disponíveis, marcando como ocupados os que estão em execução.
    /// </summary>
    Task<IReadOnlyList<AgentInfo>> GetAvailableAgentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Coloca uma nova requisição na fila de execução.
    /// </summary>
    Task EnqueueAsync(AgentExecutionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna o histórico de logs de uma issue.
    /// </summary>
    Task<IReadOnlyList<AgentLogMessage>> GetLogsAsync(string issueId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solicita o cancelamento da execução de uma issue.
    /// </summary>
    Task CancelAsync(string issueId, CancellationToken cancellationToken = default);
}
