namespace Taskboard.Dtos;

public sealed record WorkflowWorkspaceDto(
    string ProjectId,
    string Workspace,
    DateTime UpdatedAt);
