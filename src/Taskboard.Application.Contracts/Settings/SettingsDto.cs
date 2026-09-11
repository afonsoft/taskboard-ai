namespace Taskboard.Application.Contracts.Settings;

public sealed record SettingsDto(
    string Theme,
    string? GitHubToken,
    IReadOnlyList<AgentPreferenceDto> Agents);
