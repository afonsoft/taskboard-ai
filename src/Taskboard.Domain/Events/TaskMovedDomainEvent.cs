using Taskboard.ValueObjects;

namespace Taskboard.Domain.Events;

public sealed record TaskMovedDomainEvent(TaskId TaskId, string OldStatus, string NewStatus) : IDomainEvent;
