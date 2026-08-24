namespace Taskboard.Dtos;

public sealed record JiraConnectionDto(
    bool Connected,
    string? Url,
    string? Email,
    string? ProjectKey);
