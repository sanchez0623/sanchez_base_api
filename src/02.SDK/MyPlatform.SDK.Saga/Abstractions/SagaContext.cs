namespace MyPlatform.SDK.Saga.Abstractions;

/// <summary>
/// Context for saga execution.
/// </summary>
/// <typeparam name="TData">The type of saga data.</typeparam>
public class SagaContext<TData> where TData : class, new()
{
    /// <summary>
    /// Gets or sets the saga identifier.
    /// </summary>
    public string SagaId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the saga data.
    /// </summary>
    public TData Data { get; set; } = new();

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the current step index.
    /// </summary>
    public int CurrentStepIndex { get; set; }

    /// <summary>
    /// Gets or sets additional properties.
    /// </summary>
    public Dictionary<string, object?> Properties { get; set; } = [];

    /// <summary>
    /// Gets a property value.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="key">The property key.</param>
    /// <returns>The property value if found; otherwise, default.</returns>
    public T? GetProperty<T>(string key)
    {
        if (Properties.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Sets a property value.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value.</param>
    public void SetProperty<T>(string key, T value)
    {
        Properties[key] = value;
    }
}
