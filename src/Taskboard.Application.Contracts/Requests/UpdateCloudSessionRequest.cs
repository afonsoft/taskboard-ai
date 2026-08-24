namespace Taskboard.Requests;

public sealed record UpdateCloudSessionRequest(
    bool? Connected,
    string? CompanionUrl,
    string? Username,
    string? Password,
    string? ProjectId);
