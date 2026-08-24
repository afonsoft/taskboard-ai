namespace Taskboard.Requests;

public sealed record CreateRelationRequest(string TargetTaskId, string Type);
