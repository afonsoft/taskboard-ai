namespace Taskboard.Agents;

/// <summary>
/// Mensagem de log individual capturada da execução de um agente CLI.
/// </summary>
public sealed record AgentLogMessage(
    DateTimeOffset Timestamp,
    string IssueId,
    AgentLogStream Stream,
    string Content);
