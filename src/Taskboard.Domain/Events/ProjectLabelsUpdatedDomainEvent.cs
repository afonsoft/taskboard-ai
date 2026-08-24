using Taskboard.ValueObjects;

namespace Taskboard.Domain.Events;

public sealed record ProjectLabelsUpdatedDomainEvent(ProjectId ProjectId) : IDomainEvent;
