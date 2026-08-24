using Taskboard;
using Taskboard.Domain.Events;
using TaskStatus = Taskboard.ValueObjects.TaskStatus;
using Taskboard.ValueObjects;

namespace Taskboard.Domain.Entities;

public sealed class Task : AggregateRoot<TaskId>
{
    private readonly List<string> _labels = new();

    public TaskIdentifier Identifier { get; private set; } = default!;
    public ProjectId ProjectId { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public TaskStatus Status { get; private set; } = default!;
    public TaskPriority Priority { get; private set; } = default!;
    public IReadOnlyCollection<string> Labels => _labels.AsReadOnly();
    public double? SortOrder { get; private set; }
    public ThreadBinding? ThreadBinding { get; private set; }
    public Actor Creator { get; private set; } = default!;
    public Actor? Assignee { get; private set; }
    public string? WorkflowId { get; private set; }
    public string? GitBranch { get; private set; }
    public string? WorktreePath { get; private set; }
    public string? WorktreeBranch { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public Recurrence? Recurrence { get; private set; }
    public string? ExternalSource { get; private set; }
    public string? ExternalOrigin { get; private set; }
    public string? ExternalId { get; private set; }
    public string? ExternalKey { get; private set; }
    public string? ExternalUrl { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public bool IsJira => !string.IsNullOrEmpty(ExternalSource)
                          && ExternalSource.Equals("jira", StringComparison.OrdinalIgnoreCase);

    public bool IsArchived => ArchivedAt.HasValue;

    private Task()
    {
    }

    private Task(
        TaskId id,
        TaskIdentifier identifier,
        ProjectId projectId,
        string title,
        string? description,
        TaskStatus status,
        TaskPriority priority,
        IEnumerable<string>? labels,
        Actor creator,
        ThreadBinding? threadBinding,
        DateTime now,
        string? externalSource = null,
        string? externalOrigin = null,
        string? externalId = null,
        string? externalKey = null,
        string? externalUrl = null)
        : base(id)
    {
        ValidateTitle(title);

        Identifier = identifier;
        ProjectId = projectId;
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        if (labels is not null)
        {
            _labels.AddRange(labels.Distinct(StringComparer.Ordinal));
        }

        Creator = creator;
        ThreadBinding = threadBinding;
        ExternalSource = externalSource;
        ExternalOrigin = externalOrigin;
        ExternalId = externalId;
        ExternalKey = externalKey;
        ExternalUrl = externalUrl;
        CreatedAt = UpdatedAt = now;

        AddDomainEvent(new TaskCreatedDomainEvent(id, projectId));
    }

    public static Task Create(
        TaskId id,
        TaskIdentifier identifier,
        ProjectId projectId,
        string title,
        string? description,
        TaskStatus status,
        TaskPriority priority,
        Actor creator,
        IEnumerable<string>? labels = null,
        ThreadBinding? threadBinding = null,
        DateTime? now = null,
        string? externalSource = null,
        string? externalOrigin = null,
        string? externalId = null,
        string? externalKey = null,
        string? externalUrl = null)
        => new(
            id,
            identifier,
            projectId,
            title,
            description,
            status,
            priority,
            labels,
            creator,
            threadBinding,
            now ?? DateTime.UtcNow,
            externalSource,
            externalOrigin,
            externalId,
            externalKey,
            externalUrl);

    public void ApplyPatch(TaskPatch patch, long expectedVersion, DateTime? now = null)
    {
        EnsureNotArchived();
        EnsureVersion(expectedVersion);

        var changedFields = new List<string>();
        var timestamp = now ?? DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(patch.Title))
        {
            ValidateTitle(patch.Title);
            if (!string.Equals(Title, patch.Title, StringComparison.Ordinal))
            {
                Title = patch.Title;
                changedFields.Add("title");
            }
        }

        if (patch.Description is not null)
        {
            if (!string.Equals(Description, patch.Description, StringComparison.Ordinal))
            {
                Description = patch.Description;
                changedFields.Add("description");
            }
        }

        if (!string.IsNullOrWhiteSpace(patch.Status))
        {
            var newStatus = TaskStatus.From(patch.Status);
            if (!Equals(Status, newStatus))
            {
                var oldStatus = Status.Value;
                Status = newStatus;
                changedFields.Add("status");
                AddDomainEvent(new TaskMovedDomainEvent(Id, oldStatus, Status.Value));
            }
        }

        if (!string.IsNullOrWhiteSpace(patch.Priority))
        {
            var newPriority = TaskPriority.From(patch.Priority);
            if (!Equals(Priority, newPriority))
            {
                Priority = newPriority;
                changedFields.Add("priority");
            }
        }

        if (patch.Labels is not null)
        {
            var newLabels = patch.Labels.Distinct(StringComparer.Ordinal).ToList();
            _labels.Clear();
            _labels.AddRange(newLabels);
            changedFields.Add("labels");
        }

        if (patch.Assignee is not null)
        {
            if (!Equals(Assignee, patch.Assignee))
            {
                Assignee = patch.Assignee;
                changedFields.Add("assignee");
            }
        }

        if (patch.SortOrder.HasValue)
        {
            if (!SortOrder.Equals(patch.SortOrder.Value))
            {
                SortOrder = patch.SortOrder.Value;
                changedFields.Add("sort_order");
            }
        }

        if (patch.StartDate.HasValue)
        {
            StartDate = patch.StartDate.Value;
            changedFields.Add("start_date");
        }

        if (patch.DueDate.HasValue)
        {
            DueDate = patch.DueDate.Value;
            changedFields.Add("due_date");
        }

        if (!string.IsNullOrWhiteSpace(patch.WorkflowId))
        {
            if (!string.Equals(WorkflowId, patch.WorkflowId, StringComparison.Ordinal))
            {
                WorkflowId = patch.WorkflowId;
                changedFields.Add("workflow_id");
            }
        }

        if (patch.GitBranch is not null)
        {
            if (!string.Equals(GitBranch, patch.GitBranch, StringComparison.Ordinal))
            {
                GitBranch = patch.GitBranch;
                changedFields.Add("git_branch");
            }
        }

        if (patch.WorktreePath is not null)
        {
            if (!string.Equals(WorktreePath, patch.WorktreePath, StringComparison.Ordinal))
            {
                WorktreePath = patch.WorktreePath;
                changedFields.Add("worktree_path");
            }
        }

        if (patch.WorktreeBranch is not null)
        {
            if (!string.Equals(WorktreeBranch, patch.WorktreeBranch, StringComparison.Ordinal))
            {
                WorktreeBranch = patch.WorktreeBranch;
                changedFields.Add("worktree_branch");
            }
        }

        if (patch.ThreadBinding is not null)
        {
            if (!Equals(ThreadBinding, patch.ThreadBinding))
            {
                ThreadBinding = patch.ThreadBinding;
                changedFields.Add("thread_binding");
            }
        }

        if (changedFields.Count == 0)
        {
            return;
        }

        UpdatedAt = timestamp;
        IncrementVersion();
        AddDomainEvent(new TaskUpdatedDomainEvent(Id, changedFields));
    }

    public void Move(TaskStatus newStatus, double? sortOrder, Actor actor, DateTime? now = null)
    {
        EnsureNotArchived();

        var oldStatus = Status.Value;
        Status = newStatus;
        SortOrder = sortOrder;
        UpdatedAt = now ?? DateTime.UtcNow;
        IncrementVersion();

        AddDomainEvent(new TaskMovedDomainEvent(Id, oldStatus, newStatus.Value));
    }

    public void Archive(Actor actor, DateTime? now = null)
    {
        EnsureNotArchived();
        EnsureNotJira();

        ArchivedAt = now ?? DateTime.UtcNow;
        UpdatedAt = ArchivedAt.Value;
        IncrementVersion();
        AddDomainEvent(new TaskArchivedDomainEvent(Id));
    }

    public void Restore(Actor actor, DateTime? now = null)
    {
        EnsureNotJira();

        if (!ArchivedAt.HasValue)
        {
            throw new DomainException(TaskboardDomainErrorCodes.TaskNotArchived, "Task is not archived.");
        }

        ArchivedAt = null;
        UpdatedAt = now ?? DateTime.UtcNow;
        IncrementVersion();
        AddDomainEvent(new TaskRestoredDomainEvent(Id));
    }

    public void Delete(Actor actor)
    {
        EnsureNotJira();

        if (!ArchivedAt.HasValue)
        {
            throw new DomainException(TaskboardDomainErrorCodes.TaskArchived, "Only archived tasks can be deleted.");
        }

        AddDomainEvent(new TaskDeletedDomainEvent(Id));
    }

    public void SetAssignee(Actor? assignee, Actor actor, DateTime? now = null)
    {
        EnsureNotArchived();

        Assignee = assignee;
        UpdatedAt = now ?? DateTime.UtcNow;
        IncrementVersion();
        AddDomainEvent(new TaskUpdatedDomainEvent(Id, new[] { "assignee" }));
    }

    public void AddLabel(string label, Actor actor, DateTime? now = null)
    {
        EnsureNotArchived();

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
    }

    public void RemoveLabel(string label, Actor actor, DateTime? now = null)
    {
        EnsureNotArchived();

        if (!_labels.Contains(label, StringComparer.Ordinal))
        {
            return;
        }

        _labels.Remove(label);
        UpdatedAt = now ?? DateTime.UtcNow;
        IncrementVersion();
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException(TaskboardDomainErrorCodes.EmptyTaskTitle, "Task title cannot be empty.");
        }

        if (title.Length > 240)
        {
            throw new DomainException(TaskboardDomainErrorCodes.TaskTitleTooLong, "Task title cannot exceed 240 characters.");
        }
    }

    private void EnsureNotArchived()
    {
        if (ArchivedAt.HasValue)
        {
            throw new DomainException(TaskboardDomainErrorCodes.TaskArchived, "Cannot modify an archived task.");
        }
    }

    private void EnsureNotJira()
    {
        if (IsJira)
        {
            throw new DomainException(TaskboardDomainErrorCodes.TaskIsJira, "Jira tasks cannot be modified in Taskboard.");
        }
    }

    private void EnsureVersion(long expectedVersion)
    {
        if (Version != expectedVersion)
        {
            throw new DomainException(
                TaskboardDomainErrorCodes.VersionConflict,
                $"Expected version {expectedVersion} but found {Version}.");
        }
    }
}
