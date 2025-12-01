using System.Linq.Expressions;
using MyPlatform.Shared.Kernel.Domain;
using MyPlatform.Shared.Kernel.Specifications;

namespace MyPlatform.Shared.Kernel.Repositories;

/// <summary>
/// Generic repository interface for aggregate roots.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate root.</typeparam>
/// <typeparam name="TKey">The type of the aggregate root identifier.</typeparam>
public interface IRepository<TAggregate, TKey>
    where TAggregate : AggregateRoot<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Gets the unit of work associated with this repository.
    /// </summary>
    IUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Gets an aggregate by its identifier.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The aggregate if found; otherwise, null.</returns>
    Task<TAggregate?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all aggregates matching a specification.
    /// </summary>
    /// <param name="specification">The specification to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of matching aggregates.</returns>
    Task<IReadOnlyList<TAggregate>> GetAllAsync(ISpecification<TAggregate>? specification = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single aggregate matching a specification.
    /// </summary>
    /// <param name="specification">The specification to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The aggregate if found; otherwise, null.</returns>
    Task<TAggregate?> GetSingleAsync(ISpecification<TAggregate> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any aggregate matches the specification.
    /// </summary>
    /// <param name="specification">The specification to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if any aggregate matches; otherwise, false.</returns>
    Task<bool> AnyAsync(ISpecification<TAggregate>? specification = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts aggregates matching the specification.
    /// </summary>
    /// <param name="specification">The specification to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of matching aggregates.</returns>
    Task<int> CountAsync(ISpecification<TAggregate>? specification = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added aggregate.</returns>
    Task<TAggregate> AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple aggregates to the repository.
    /// </summary>
    /// <param name="aggregates">The aggregates to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddRangeAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing aggregate.
    /// </summary>
    /// <param name="aggregate">The aggregate to update.</param>
    void Update(TAggregate aggregate);

    /// <summary>
    /// Removes an aggregate from the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to remove.</param>
    void Remove(TAggregate aggregate);

    /// <summary>
    /// Removes multiple aggregates from the repository.
    /// </summary>
    /// <param name="aggregates">The aggregates to remove.</param>
    void RemoveRange(IEnumerable<TAggregate> aggregates);
}

/// <summary>
/// Generic repository interface for aggregate roots with long identifier.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate root.</typeparam>
public interface IRepository<TAggregate> : IRepository<TAggregate, long>
    where TAggregate : AggregateRoot<long>
{
}
