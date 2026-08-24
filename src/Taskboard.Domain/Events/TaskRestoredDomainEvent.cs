using Taskboard.ValueObjects;

namespace Taskboard.Domain.Events;

public sealed record TaskRestoredDomainEvent(TaskId TaskId) : IDomainEvent;
