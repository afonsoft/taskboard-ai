using Volo.Abp.Domain.Entities;

namespace Taskboard;

public abstract class AggregateRoot<TKey> : BasicAggregateRoot<TKey>, IEntity<TKey>
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

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
        AddLocalEvent(domainEvent);
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
        ClearLocalEvents();
    }

    protected void IncrementVersion()
    {
        Version++;
    }
}
