namespace Taskboard.Agents;

/// <summary>
/// Adaptador que traduz uma requisição ACP para o comando específico de um CLI de agente.
/// </summary>
public interface IAgentAdapter
{
    /// <summary>
    /// Indica se o adaptador sabe lidar com o tipo de agente.
    /// </summary>
    bool CanHandle(AgentType agentType);

    /// <summary>
    /// Monta o comando local a ser executado a partir da requisição.
    /// </summary>
    AgentCommand BuildCommand(AgentExecutionRequest request);
}
