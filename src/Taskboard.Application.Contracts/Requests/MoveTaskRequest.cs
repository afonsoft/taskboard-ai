namespace Taskboard.Requests;

public sealed record MoveTaskRequest(string Status, double? SortOrder);
