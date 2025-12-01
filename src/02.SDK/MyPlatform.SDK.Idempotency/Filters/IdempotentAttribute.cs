using Microsoft.AspNetCore.Mvc;

namespace MyPlatform.SDK.Idempotency.Filters;

/// <summary>
/// Attribute to mark an action as requiring idempotency.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class IdempotentAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdempotentAttribute"/> class.
    /// </summary>
    public IdempotentAttribute() : base(typeof(IdempotencyFilter))
    {
    }

    /// <summary>
    /// Gets or sets the custom expiration time in seconds.
    /// </summary>
    public int ExpirationSeconds { get; set; }
}
