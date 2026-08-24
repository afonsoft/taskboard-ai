namespace Taskboard.Requests;

public sealed record UpdateJiraConnectionRequest(
    string? Url,
    string? Email,
    string? Token,
    string? ProjectKey);
