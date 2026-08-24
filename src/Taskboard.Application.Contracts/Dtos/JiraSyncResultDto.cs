namespace Taskboard.Dtos;

public sealed record JiraSyncResultDto(
    bool Success,
    int Imported,
    int Updated,
    int Archived);
