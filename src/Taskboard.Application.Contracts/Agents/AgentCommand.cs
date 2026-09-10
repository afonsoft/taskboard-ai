namespace Taskboard.Agents;

/// <summary>
/// Comando local a ser executado por um adaptador de agente.
/// </summary>
public sealed record AgentCommand(
    string ExecutablePath,
    IReadOnlyList<string> Arguments,
    string WorkingDirectory);
