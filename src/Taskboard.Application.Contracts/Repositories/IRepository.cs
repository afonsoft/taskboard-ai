namespace Taskboard.Repositories;

public interface IRepository<T>
    where T : class
{
    IQueryable<T> Query { get; }

    Task<T?> GetAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
        where TKey : notnull;

    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
