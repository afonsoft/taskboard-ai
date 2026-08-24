namespace Taskboard.Dtos;

public sealed record TaskListDto(IReadOnlyList<TaskDto> Tasks, ProjectDto? Project);
