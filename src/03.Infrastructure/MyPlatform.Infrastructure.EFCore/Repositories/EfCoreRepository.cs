using Microsoft.EntityFrameworkCore;
using MyPlatform.Shared.Kernel.Domain;
using MyPlatform.Shared.Kernel.Repositories;
using MyPlatform.Shared.Kernel.Specifications;

namespace MyPlatform.Infrastructure.EFCore.Repositories;

/// <summary>
/// EF Core implementation of the generic repository.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate root.</typeparam>
/// <typeparam name="TKey">The type of the aggregate root identifier.</typeparam>
/// <typeparam name="TContext">The type of DbContext.</typeparam>
public class EfCoreRepository<TAggregate, TKey, TContext> : IRepository<TAggregate, TKey>
    where TAggregate : AggregateRoot<TKey>
    where TKey : notnull
    where TContext : DbContext
{
    protected readonly TContext Context;
    protected readonly DbSet<TAggregate> DbSet;

    public EfCoreRepository(TContext context, IUnitOfWork unitOfWork)
    {
        Context = context;
        DbSet = context.Set<TAggregate>();
        UnitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public IUnitOfWork UnitOfWork { get; }

    /// <inheritdoc />
    public virtual async Task<TAggregate?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TAggregate>> GetAllAsync(ISpecification<TAggregate>? specification = null, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TAggregate?> GetSingleAsync(ISpecification<TAggregate> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(ISpecification<TAggregate>? specification = null, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(ISpecification<TAggregate>? specification = null, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TAggregate> AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var entry = await DbSet.AddAsync(aggregate, cancellationToken);
        return entry.Entity;
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(aggregates, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Update(TAggregate aggregate)
    {
        DbSet.Update(aggregate);
    }

    /// <inheritdoc />
    public virtual void Remove(TAggregate aggregate)
    {
        DbSet.Remove(aggregate);
    }

    /// <inheritdoc />
    public virtual void RemoveRange(IEnumerable<TAggregate> aggregates)
    {
        DbSet.RemoveRange(aggregates);
    }

    /// <summary>
    /// Applies a specification to the query.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>The query with specification applied.</returns>
    protected virtual IQueryable<TAggregate> ApplySpecification(ISpecification<TAggregate>? specification)
    {
        if (specification is null)
        {
            return DbSet;
        }

        IQueryable<TAggregate> query = DbSet;

        // Apply includes
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply criteria
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply ordering
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            if (specification.Skip.HasValue)
            {
                query = query.Skip(specification.Skip.Value);
            }
            if (specification.Take.HasValue)
            {
                query = query.Take(specification.Take.Value);
            }
        }

        // Apply tracking
        if (specification.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        // Apply split query
        if (specification.AsSplitQuery)
        {
            query = query.AsSplitQuery();
        }

        return query;
    }
}

/// <summary>
/// EF Core repository for aggregate roots with long identifier.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate root.</typeparam>
/// <typeparam name="TContext">The type of DbContext.</typeparam>
public class EfCoreRepository<TAggregate, TContext> : EfCoreRepository<TAggregate, long, TContext>, IRepository<TAggregate>
    where TAggregate : AggregateRoot<long>
    where TContext : DbContext
{
    public EfCoreRepository(TContext context, IUnitOfWork unitOfWork)
        : base(context, unitOfWork)
    {
    }
}
