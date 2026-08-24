namespace Taskboard.Dtos;

public sealed record CloudSessionDto(
    bool Connected,
    string? CompanionUrl,
    string? Username,
    string? ProjectId);
