using Taskboard.ValueObjects;

namespace Taskboard.Requests;

public sealed record UpdateTaskRequest(long Version, TaskPatch Changes);
