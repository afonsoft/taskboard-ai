using Taskboard.ValueObjects;

namespace Taskboard.Domain.Events;

public sealed record TaskUpdatedDomainEvent(TaskId TaskId, IReadOnlyList<string> ChangedFields) : IDomainEvent;
