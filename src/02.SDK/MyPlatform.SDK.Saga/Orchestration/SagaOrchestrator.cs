using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.Saga.Abstractions;
using MyPlatform.SDK.Saga.Configuration;
using MyPlatform.SDK.Saga.Models;
using MyPlatform.SDK.Saga.Persistence;
using Newtonsoft.Json;

namespace MyPlatform.SDK.Saga.Orchestration;

/// <summary>
/// Result of saga execution.
/// </summary>
/// <typeparam name="TData">The type of saga data.</typeparam>
public class SagaResult<TData> where TData : class, new()
{
    /// <summary>
    /// Gets or sets a value indicating whether the saga succeeded.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the saga identifier.
    /// </summary>
    public string SagaId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the final status.
    /// </summary>
    public SagaStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the saga data.
    /// </summary>
    public TData? Data { get; set; }

    /// <summary>
    /// Gets or sets the error message if failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static SagaResult<TData> Success(string sagaId, TData data) => new()
    {
        IsSuccess = true,
        SagaId = sagaId,
        Status = SagaStatus.Completed,
        Data = data
    };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static SagaResult<TData> Failure(string sagaId, SagaStatus status, string? error = null) => new()
    {
        IsSuccess = false,
        SagaId = sagaId,
        Status = status,
        ErrorMessage = error
    };
}

/// <summary>
/// Interface for saga orchestrator.
/// </summary>
/// <typeparam name="TData">The type of saga data.</typeparam>
public interface ISagaOrchestrator<TData> where TData : class, new()
{
    /// <summary>
    /// Executes a saga with the provided data.
    /// </summary>
    /// <param name="data">The initial saga data.</param>
    /// <param name="correlationId">Optional correlation identifier.</param>
    /// <param name="tenantId">Optional tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saga execution result.</returns>
    Task<SagaResult<TData>> ExecuteAsync(TData data, string? correlationId = null, string? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a suspended or failed saga.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saga execution result.</returns>
    Task<SagaResult<TData>> ResumeAsync(string sagaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually compensates a saga.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saga execution result.</returns>
    Task<SagaResult<TData>> CompensateAsync(string sagaId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base class for saga orchestrators.
/// </summary>
/// <typeparam name="TData">The type of saga data.</typeparam>
public abstract class SagaOrchestrator<TData> : ISagaOrchestrator<TData> where TData : class, new()
{
    private readonly ISagaStateStore _stateStore;
    private readonly SagaOptions _options;
    private readonly ILogger _logger;
    private readonly List<ISagaStep<TData>> _steps = [];

    protected SagaOrchestrator(
        ISagaStateStore stateStore,
        IOptions<SagaOptions> options,
        ILogger logger)
    {
        _stateStore = stateStore;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Gets the saga name.
    /// </summary>
    protected abstract string SagaName { get; }

    /// <summary>
    /// Configures the saga steps.
    /// </summary>
    protected abstract void ConfigureSteps();

    /// <summary>
    /// Adds a step to the saga.
    /// </summary>
    /// <param name="step">The step to add.</param>
    protected void AddStep(ISagaStep<TData> step)
    {
        _steps.Add(step);
    }

    /// <inheritdoc />
    public async Task<SagaResult<TData>> ExecuteAsync(TData data, string? correlationId = null, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        ConfigureSteps();

        var context = new SagaContext<TData>
        {
            Data = data,
            CorrelationId = correlationId,
            TenantId = tenantId
        };

        var state = new SagaState
        {
            SagaId = context.SagaId,
            SagaName = SagaName,
            Status = SagaStatus.Running,
            CorrelationId = correlationId,
            TenantId = tenantId,
            Data = JsonConvert.SerializeObject(data)
        };

        // Initialize step states
        for (var i = 0; i < _steps.Count; i++)
        {
            state.Steps.Add(new SagaStepState
            {
                StepIndex = i,
                StepName = _steps[i].StepName,
                Status = SagaStepStatus.Pending
            });
        }

        await _stateStore.SaveAsync(state, cancellationToken);

        _logger.LogInformation("Starting saga {SagaName} with ID {SagaId}", SagaName, context.SagaId);

        return await ExecuteStepsAsync(context, state, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SagaResult<TData>> ResumeAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        ConfigureSteps();

        var state = await _stateStore.GetAsync(sagaId, cancellationToken);
        if (state is null)
        {
            return SagaResult<TData>.Failure(sagaId, SagaStatus.Failed, "Saga not found");
        }

        var data = string.IsNullOrEmpty(state.Data)
            ? new TData()
            : JsonConvert.DeserializeObject<TData>(state.Data) ?? new TData();

        var context = new SagaContext<TData>
        {
            SagaId = sagaId,
            Data = data,
            CorrelationId = state.CorrelationId,
            TenantId = state.TenantId,
            CurrentStepIndex = state.CurrentStepIndex
        };

        state.Status = SagaStatus.Running;
        state.NextRetryAt = null;
        await _stateStore.SaveAsync(state, cancellationToken);

        _logger.LogInformation("Resuming saga {SagaName} with ID {SagaId} from step {StepIndex}", SagaName, sagaId, state.CurrentStepIndex);

        return await ExecuteStepsAsync(context, state, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SagaResult<TData>> CompensateAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        ConfigureSteps();

        var state = await _stateStore.GetAsync(sagaId, cancellationToken);
        if (state is null)
        {
            return SagaResult<TData>.Failure(sagaId, SagaStatus.Failed, "Saga not found");
        }

        var data = string.IsNullOrEmpty(state.Data)
            ? new TData()
            : JsonConvert.DeserializeObject<TData>(state.Data) ?? new TData();

        var context = new SagaContext<TData>
        {
            SagaId = sagaId,
            Data = data,
            CorrelationId = state.CorrelationId,
            TenantId = state.TenantId
        };

        state.Status = SagaStatus.Compensating;
        await _stateStore.SaveAsync(state, cancellationToken);

        _logger.LogInformation("Manually compensating saga {SagaName} with ID {SagaId}", SagaName, sagaId);

        return await CompensateStepsAsync(context, state, state.CurrentStepIndex, cancellationToken);
    }

    private async Task<SagaResult<TData>> ExecuteStepsAsync(SagaContext<TData> context, SagaState state, CancellationToken cancellationToken)
    {
        for (var i = state.CurrentStepIndex; i < _steps.Count; i++)
        {
            var step = _steps[i];
            var stepState = state.Steps[i];

            context.CurrentStepIndex = i;
            state.CurrentStepIndex = i;
            stepState.Status = SagaStepStatus.Executing;
            stepState.StartedAt = DateTime.UtcNow;

            _logger.LogDebug("Executing step {StepName} ({StepIndex}/{TotalSteps}) for saga {SagaId}",
                step.StepName, i + 1, _steps.Count, context.SagaId);

            try
            {
                await step.ExecuteAsync(context, cancellationToken);

                stepState.Status = SagaStepStatus.Completed;
                stepState.CompletedAt = DateTime.UtcNow;
                state.Data = JsonConvert.SerializeObject(context.Data);

                await _stateStore.SaveAsync(state, cancellationToken);

                _logger.LogDebug("Step {StepName} completed successfully for saga {SagaId}", step.StepName, context.SagaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Step {StepName} failed for saga {SagaId}: {Error}", step.StepName, context.SagaId, ex.Message);

                stepState.Status = SagaStepStatus.Failed;
                stepState.ErrorMessage = ex.Message;
                stepState.RetryCount++;
                state.LastError = ex.Message;
                state.RetryCount++;

                if (stepState.RetryCount < _options.DefaultRetryCount)
                {
                    // Schedule retry
                    var delay = CalculateRetryDelay(stepState.RetryCount);
                    state.NextRetryAt = DateTime.UtcNow.AddSeconds(delay);
                    state.Status = SagaStatus.Suspended;
                    await _stateStore.SaveAsync(state, cancellationToken);

                    return SagaResult<TData>.Failure(context.SagaId, SagaStatus.Suspended, ex.Message);
                }

                // Max retries exceeded, start compensation
                return await CompensateStepsAsync(context, state, i - 1, cancellationToken);
            }
        }

        // All steps completed successfully
        state.Status = SagaStatus.Completed;
        state.CompletedAt = DateTime.UtcNow;
        await _stateStore.SaveAsync(state, cancellationToken);

        _logger.LogInformation("Saga {SagaName} with ID {SagaId} completed successfully", SagaName, context.SagaId);

        return SagaResult<TData>.Success(context.SagaId, context.Data);
    }

    private async Task<SagaResult<TData>> CompensateStepsAsync(SagaContext<TData> context, SagaState state, int fromStepIndex, CancellationToken cancellationToken)
    {
        state.Status = SagaStatus.Compensating;
        await _stateStore.SaveAsync(state, cancellationToken);

        var compensationFailed = false;

        for (var i = fromStepIndex; i >= 0; i--)
        {
            var step = _steps[i];
            var stepState = state.Steps[i];

            if (stepState.Status != SagaStepStatus.Completed)
            {
                continue;
            }

            stepState.Status = SagaStepStatus.Compensating;
            _logger.LogDebug("Compensating step {StepName} for saga {SagaId}", step.StepName, context.SagaId);

            try
            {
                await step.CompensateAsync(context, cancellationToken);
                stepState.Status = SagaStepStatus.Compensated;

                _logger.LogDebug("Step {StepName} compensated successfully for saga {SagaId}", step.StepName, context.SagaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Compensation failed for step {StepName} in saga {SagaId}: {Error}",
                    step.StepName, context.SagaId, ex.Message);

                stepState.Status = SagaStepStatus.CompensationFailed;
                stepState.ErrorMessage = ex.Message;
                compensationFailed = true;
            }

            await _stateStore.SaveAsync(state, cancellationToken);
        }

        if (compensationFailed)
        {
            state.Status = SagaStatus.Failed;
            await _stateStore.SaveAsync(state, cancellationToken);

            _logger.LogError("Saga {SagaName} with ID {SagaId} failed with incomplete compensation", SagaName, context.SagaId);

            return SagaResult<TData>.Failure(context.SagaId, SagaStatus.Failed, "Compensation failed - manual intervention required");
        }

        state.Status = SagaStatus.Compensated;
        state.CompletedAt = DateTime.UtcNow;
        await _stateStore.SaveAsync(state, cancellationToken);

        _logger.LogInformation("Saga {SagaName} with ID {SagaId} compensated successfully", SagaName, context.SagaId);

        return SagaResult<TData>.Failure(context.SagaId, SagaStatus.Compensated, state.LastError);
    }

    private double CalculateRetryDelay(int retryCount)
    {
        var delay = _options.InitialRetryDelaySeconds * Math.Pow(_options.RetryDelayMultiplier, retryCount - 1);
        return Math.Min(delay, _options.MaxRetryDelaySeconds);
    }
}
