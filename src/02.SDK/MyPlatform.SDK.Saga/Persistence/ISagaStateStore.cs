using MyPlatform.SDK.Saga.Models;

namespace MyPlatform.SDK.Saga.Persistence;

/// <summary>
/// Interface for persisting saga state.
/// </summary>
public interface ISagaStateStore
{
    /// <summary>
    /// Saves a saga state.
    /// </summary>
    /// <param name="state">The saga state to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    Task SaveAsync(SagaState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a saga state by its identifier.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saga state if found; otherwise, null.</returns>
    Task<SagaState?> GetAsync(string sagaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sagas with a specific status.
    /// </summary>
    /// <param name="status">The status to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of saga states.</returns>
    Task<IEnumerable<SagaState>> GetByStatusAsync(SagaStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sagas that need to be retried.
    /// </summary>
    /// <param name="batchSize">The maximum number of sagas to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of saga states.</returns>
    Task<IEnumerable<SagaState>> GetPendingRetriesAsync(int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a saga state.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    Task DeleteAsync(string sagaId, CancellationToken cancellationToken = default);
}
