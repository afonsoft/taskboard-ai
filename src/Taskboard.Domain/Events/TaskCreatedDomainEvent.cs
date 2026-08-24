using Taskboard.ValueObjects;

namespace Taskboard.Domain.Events;

public sealed record TaskCreatedDomainEvent(TaskId TaskId, ProjectId ProjectId) : IDomainEvent;
