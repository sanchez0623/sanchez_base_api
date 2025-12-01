namespace MyPlatform.SDK.Saga.Models;

/// <summary>
/// Status of a saga execution.
/// </summary>
public enum SagaStatus
{
    /// <summary>
    /// Saga has been created but not started.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Saga is currently running.
    /// </summary>
    Running = 1,

    /// <summary>
    /// Saga completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Saga failed and compensation is in progress.
    /// </summary>
    Compensating = 3,

    /// <summary>
    /// Saga failed and compensation completed.
    /// </summary>
    Compensated = 4,

    /// <summary>
    /// Saga failed and requires manual intervention.
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Saga was suspended and requires manual intervention.
    /// </summary>
    Suspended = 6
}

/// <summary>
/// Status of a saga step execution.
/// </summary>
public enum SagaStepStatus
{
    /// <summary>
    /// Step is pending execution.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Step is currently executing.
    /// </summary>
    Executing = 1,

    /// <summary>
    /// Step completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Step execution failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Step is being compensated.
    /// </summary>
    Compensating = 4,

    /// <summary>
    /// Step compensation completed.
    /// </summary>
    Compensated = 5,

    /// <summary>
    /// Step compensation failed.
    /// </summary>
    CompensationFailed = 6,

    /// <summary>
    /// Step was skipped.
    /// </summary>
    Skipped = 7
}
