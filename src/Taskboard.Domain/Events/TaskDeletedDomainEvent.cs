using Taskboard.ValueObjects;

namespace Taskboard.Domain.Events;

public sealed record TaskDeletedDomainEvent(TaskId TaskId) : IDomainEvent;
