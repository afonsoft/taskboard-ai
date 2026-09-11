using Taskboard.Agents;

namespace Taskboard.Application.Contracts.Settings;

public sealed record AgentPreferenceDto(
    AgentType Type,
    string Name,
    string? ExecutablePath,
    string? Version,
    bool Enabled);
