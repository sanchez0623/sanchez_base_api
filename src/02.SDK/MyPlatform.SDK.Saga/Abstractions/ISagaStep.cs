namespace MyPlatform.SDK.Saga.Abstractions;

/// <summary>
/// Base interface for saga steps.
/// </summary>
/// <typeparam name="TData">The type of saga data.</typeparam>
public interface ISagaStep<TData> where TData : class, new()
{
    /// <summary>
    /// Gets the step name.
    /// </summary>
    string StepName { get; }

    /// <summary>
    /// Executes the saga step.
    /// </summary>
    /// <param name="context">The saga context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    Task ExecuteAsync(SagaContext<TData> context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compensates for the saga step execution.
    /// </summary>
    /// <param name="context">The saga context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    Task CompensateAsync(SagaContext<TData> context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base class for saga steps with default implementation.
/// </summary>
/// <typeparam name="TData">The type of saga data.</typeparam>
public abstract class SagaStepBase<TData> : ISagaStep<TData> where TData : class, new()
{
    /// <inheritdoc />
    public virtual string StepName => GetType().Name;

    /// <inheritdoc />
    public abstract Task ExecuteAsync(SagaContext<TData> context, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public virtual Task CompensateAsync(SagaContext<TData> context, CancellationToken cancellationToken = default)
    {
        // Default implementation does nothing
        return Task.CompletedTask;
    }
}
