namespace Taskboard;

public abstract class AggregateRoot<TKey> : Entity<TKey>
    where TKey : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public long Version { get; protected set; } = 1;

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TKey id)
        : base(id)
    {
    }

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void IncrementVersion()
    {
        Version++;
    }
}
