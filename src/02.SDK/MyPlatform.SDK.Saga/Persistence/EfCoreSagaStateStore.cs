using Microsoft.EntityFrameworkCore;
using MyPlatform.SDK.Saga.Models;
using Newtonsoft.Json;

namespace MyPlatform.SDK.Saga.Persistence;

/// <summary>
/// Entity Framework Core implementation of saga state store.
/// </summary>
public class EfCoreSagaStateStore : ISagaStateStore
{
    private readonly SagaDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfCoreSagaStateStore"/> class.
    /// </summary>
    /// <param name="dbContext">The saga DbContext.</param>
    public EfCoreSagaStateStore(SagaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task SaveAsync(SagaState state, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.SagaStates
            .FirstOrDefaultAsync(e => e.SagaId == state.SagaId, cancellationToken);

        if (entity == null)
        {
            entity = new SagaStateEntity
            {
                SagaId = state.SagaId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.SagaStates.Add(entity);
        }

        entity.SagaType = state.SagaName;
        entity.State = state.Status.ToString();
        entity.Data = state.Data;
        entity.CurrentStep = state.CurrentStepIndex;
        entity.LastError = state.LastError;
        entity.RetryCount = state.RetryCount;
        entity.TenantId = state.TenantId;
        entity.CorrelationId = state.CorrelationId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.CompletedAt = state.CompletedAt;
        entity.NextRetryAt = state.NextRetryAt;
        entity.Steps = state.Steps.Count > 0 ? JsonConvert.SerializeObject(state.Steps) : null;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SagaState?> GetAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.SagaStates
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.SagaId == sagaId, cancellationToken);

        return entity == null ? null : MapToSagaState(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SagaState>> GetByStatusAsync(SagaStatus status, CancellationToken cancellationToken = default)
    {
        var statusString = status.ToString();
        var entities = await _dbContext.SagaStates
            .AsNoTracking()
            .Where(e => e.State == statusString)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToSagaState);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SagaState>> GetPendingRetriesAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entities = await _dbContext.SagaStates
            .AsNoTracking()
            .Where(e => e.NextRetryAt.HasValue && e.NextRetryAt <= now)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToSagaState);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.SagaStates
            .FirstOrDefaultAsync(e => e.SagaId == sagaId, cancellationToken);

        if (entity != null)
        {
            _dbContext.SagaStates.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static SagaState MapToSagaState(SagaStateEntity entity)
    {
        var state = new SagaState
        {
            SagaId = entity.SagaId,
            SagaName = entity.SagaType,
            Status = Enum.TryParse<SagaStatus>(entity.State, out var status) ? status : SagaStatus.Pending,
            Data = entity.Data,
            CurrentStepIndex = entity.CurrentStep,
            LastError = entity.LastError,
            RetryCount = entity.RetryCount,
            TenantId = entity.TenantId,
            CorrelationId = entity.CorrelationId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CompletedAt = entity.CompletedAt,
            NextRetryAt = entity.NextRetryAt
        };

        if (!string.IsNullOrEmpty(entity.Steps))
        {
            state.Steps = JsonConvert.DeserializeObject<List<SagaStepState>>(entity.Steps) ?? [];
        }

        return state;
    }
}
