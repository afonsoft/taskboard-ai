using Taskboard;
using Taskboard.Domain.Events;
using Taskboard.ValueObjects;

namespace Taskboard.Domain.Entities;

public sealed class Project : AggregateRoot<ProjectId>
{
    private readonly List<string> _labels = new();

    public string Name { get; private set; } = default!;
    public string? WorkspacePath { get; private set; }
    public IReadOnlyCollection<string> Labels => _labels.AsReadOnly();
    public long NextTaskNumber { get; private set; } = 1;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Project()
    {
    }

    private Project(ProjectId id, string name, string? workspacePath, DateTime now)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(TaskboardDomainErrorCodes.EmptyProjectName, "Project name cannot be empty.");
        }

        Name = name;
        WorkspacePath = workspacePath;
        CreatedAt = UpdatedAt = now;
    }

    public static Project Create(ProjectId id, string name, string? workspacePath = null, DateTime? now = null)
        => new(id, name, workspacePath, now ?? DateTime.UtcNow);

    public static Project Local(DateTime? now = null)
        => new(ProjectId.From("local"), "全局", null, now ?? DateTime.UtcNow);

    public TaskIdentifier GenerateTaskIdentifier()
    {
        var identifier = TaskIdentifier.ForLocalTask(Id, NextTaskNumber);
        NextTaskNumber++;
        IncrementVersion();
        return identifier;
    }

    public void Rename(string name, DateTime? now = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(TaskboardDomainErrorCodes.EmptyProjectName, "Project name cannot be empty.");
        }

        Name = name;
        UpdatedAt = now ?? DateTime.UtcNow;
        IncrementVersion();
    }

    public void AddLabel(string label, DateTime? now = null)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new DomainException(TaskboardDomainErrorCodes.InvalidValue, "Label cannot be empty.");
        }

        if (_labels.Contains(label, StringComparer.Ordinal))
        {
            return;
        }

        _labels.Add(label);
        UpdatedAt = now ?? DateTime.UtcNow;
        IncrementVersion();
        AddDomainEvent(new ProjectLabelsUpdatedDomainEvent(Id));
    }

    public void RemoveLabel(string label, DateTime? now = null)
    {
        if (!_labels.Contains(label, StringComparer.Ordinal))
        {
            return;
        }

        _labels.Remove(label);
        UpdatedAt = now ?? DateTime.UtcNow;
        IncrementVersion();
        AddDomainEvent(new ProjectLabelsUpdatedDomainEvent(Id));
    }

    public void UpdateWorkspacePath(string? workspacePath, DateTime? now = null)
    {
        WorkspacePath = workspacePath;
        UpdatedAt = now ?? DateTime.UtcNow;
        IncrementVersion();
    }
}
