namespace Taskboard.Requests;

public sealed record SaveSettingsRequest(
    string Theme,
    string? GitHubToken,
    IReadOnlyList<string> EnabledAgents);
