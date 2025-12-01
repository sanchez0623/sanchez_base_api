using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MyPlatform.Shared.Kernel.Domain;
using MyPlatform.Shared.Kernel.Events;
using MyPlatform.Shared.Kernel.Repositories;

namespace MyPlatform.Infrastructure.EFCore.UnitOfWork;

/// <summary>
/// EF Core implementation of Unit of Work.
/// </summary>
public class EfCoreUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private IDbContextTransaction? _currentTransaction;

    public EfCoreUnitOfWork(TContext context, IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    /// <inheritdoc />
    public bool HasActiveTransaction => _currentTransaction is not null;

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        var aggregates = _context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity is AggregateRoot<long>)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = new List<IDomainEvent>();
        foreach (var aggregate in aggregates)
        {
            if (aggregate is AggregateRoot<long> aggregateRoot)
            {
                domainEvents.AddRange(aggregateRoot.DomainEvents);
                aggregateRoot.ClearDomainEvents();
            }
        }

        var result = await _context.SaveChangesAsync(cancellationToken);

        // Dispatch events after successful save
        if (domainEvents.Count != 0)
        {
            await _eventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            throw new InvalidOperationException("No transaction has been started.");
        }

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
