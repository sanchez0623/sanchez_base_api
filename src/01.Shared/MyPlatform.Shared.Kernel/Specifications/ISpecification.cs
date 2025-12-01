using System.Linq.Expressions;

namespace MyPlatform.Shared.Kernel.Specifications;

/// <summary>
/// Specification pattern interface for building complex queries.
/// </summary>
/// <typeparam name="T">The type of entity this specification applies to.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the criteria expression for filtering entities.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Gets the include expressions for eager loading related entities.
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets the string-based include expressions for eager loading related entities.
    /// </summary>
    IReadOnlyList<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the order by expression for sorting entities.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Gets the order by descending expression for sorting entities.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Gets the number of entities to skip for pagination.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Gets the number of entities to take for pagination.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Gets a value indicating whether pagination is enabled.
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether tracking is disabled for performance.
    /// </summary>
    bool AsNoTracking { get; }

    /// <summary>
    /// Gets a value indicating whether split query mode should be used.
    /// </summary>
    bool AsSplitQuery { get; }
}
