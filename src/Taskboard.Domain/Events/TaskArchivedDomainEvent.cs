using Taskboard.ValueObjects;

namespace Taskboard.Domain.Events;

public sealed record TaskArchivedDomainEvent(TaskId TaskId) : IDomainEvent;
