using Taskboard;
using Taskboard.ValueObjects;
using Volo.Abp.Domain.Entities;

namespace Taskboard.Domain.Entities;

public sealed class WorkflowWorkspace : Entity<ProjectId>
{
    public string Workspace { get; private set; } = default!;
    public DateTime UpdatedAt { get; private set; }

    private WorkflowWorkspace()
    {
    }

    private WorkflowWorkspace(ProjectId id, string workspace, DateTime updatedAt)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(workspace))
        {
            throw new DomainException(TaskboardDomainErrorCodes.InvalidValue, "Workspace cannot be empty.");
        }

        Workspace = workspace;
        UpdatedAt = updatedAt;
    }

    public static WorkflowWorkspace Create(ProjectId projectId, string workspace, DateTime? now = null)
        => new(projectId, workspace, now ?? DateTime.UtcNow);

    public void Update(string workspace, DateTime? now = null)
    {
        if (string.IsNullOrWhiteSpace(workspace))
        {
            throw new DomainException(TaskboardDomainErrorCodes.InvalidValue, "Workspace cannot be empty.");
        }

        Workspace = workspace;
        UpdatedAt = now ?? DateTime.UtcNow;
    }
}
