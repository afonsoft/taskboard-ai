using Taskboard.ValueObjects;
using Volo.Abp.Domain.Entities;

namespace Taskboard.Domain.Entities;

public sealed class ProjectSummary : Entity<ProjectId>
{
    public string Summary { get; private set; } = default!;
    public DateTime GeneratedAt { get; private set; }

    private ProjectSummary()
    {
    }

    private ProjectSummary(ProjectId id, string summary, DateTime generatedAt)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            throw new ArgumentException("Summary cannot be empty.", nameof(summary));
        }

        Summary = summary;
        GeneratedAt = generatedAt;
    }

    public static ProjectSummary Create(ProjectId projectId, string summary, DateTime? generatedAt = null)
        => new(projectId, summary, generatedAt ?? DateTime.UtcNow);
}
