namespace Taskboard.Agents;

/// <summary>
/// Resultado da execução de um agente CLI.
/// </summary>
public sealed record AgentExecutionResult(int ExitCode, bool IsSuccess);
