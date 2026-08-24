using Taskboard.ValueObjects;

namespace Taskboard.Domain.Events;

public sealed record CommentAddedDomainEvent(TaskId TaskId, CommentId CommentId) : IDomainEvent;
