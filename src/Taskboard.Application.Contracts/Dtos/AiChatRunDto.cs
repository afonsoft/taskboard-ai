namespace Taskboard.Dtos;

public sealed record AiChatRunDto(
    string Id,
    string ThreadId,
    string Status,
    int? ExitCode,
    DateTime CreatedAt,
    DateTime? FinishedAt);
