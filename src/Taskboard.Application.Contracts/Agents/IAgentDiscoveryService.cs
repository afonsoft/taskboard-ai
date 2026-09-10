namespace Taskboard.Agents;

/// <summary>
/// Localiza e descreve os agentes CLI instalados no servidor.
/// </summary>
public interface IAgentDiscoveryService
{
    /// <summary>
    /// Retorna todos os agentes conhecidos com seus status de disponibilidade.
    /// </summary>
    Task<IReadOnlyList<AgentInfo>> DiscoverAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolve o caminho completo do executável para o tipo de agente informado.
    /// </summary>
    string? ResolveExecutablePath(AgentType agentType);
}
