using System.Linq.Expressions;

namespace MyPlatform.Shared.Kernel.Specifications;

/// <summary>
/// Base implementation of the specification pattern.
/// </summary>
/// <typeparam name="T">The type of entity this specification applies to.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, object>>> _includes = [];
    private readonly List<string> _includeStrings = [];

    /// <inheritdoc />
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes.AsReadOnly();

    /// <inheritdoc />
    public IReadOnlyList<string> IncludeStrings => _includeStrings.AsReadOnly();

    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <inheritdoc />
    public int? Skip { get; private set; }

    /// <inheritdoc />
    public int? Take { get; private set; }

    /// <inheritdoc />
    public bool IsPagingEnabled { get; private set; }

    /// <inheritdoc />
    public bool AsNoTracking { get; private set; } = true;

    /// <inheritdoc />
    public bool AsSplitQuery { get; private set; }

    /// <summary>
    /// Sets the filter criteria for this specification.
    /// </summary>
    /// <param name="criteria">The filter expression.</param>
    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <summary>
    /// Adds an include expression for eager loading.
    /// </summary>
    /// <param name="includeExpression">The include expression.</param>
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        _includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds a string-based include expression for eager loading.
    /// </summary>
    /// <param name="includeString">The include string.</param>
    protected void AddInclude(string includeString)
    {
        _includeStrings.Add(includeString);
    }

    /// <summary>
    /// Sets the order by expression.
    /// </summary>
    /// <param name="orderByExpression">The order by expression.</param>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Sets the order by descending expression.
    /// </summary>
    /// <param name="orderByDescendingExpression">The order by descending expression.</param>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    /// <summary>
    /// Applies pagination to this specification.
    /// </summary>
    /// <param name="skip">The number of entities to skip.</param>
    /// <param name="take">The number of entities to take.</param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    /// <summary>
    /// Enables or disables tracking for this specification.
    /// </summary>
    /// <param name="asNoTracking">True to disable tracking; false to enable.</param>
    protected void ApplyAsNoTracking(bool asNoTracking = true)
    {
        AsNoTracking = asNoTracking;
    }

    /// <summary>
    /// Enables or disables split query mode for this specification.
    /// </summary>
    /// <param name="asSplitQuery">True to enable split query; false to disable.</param>
    protected void ApplyAsSplitQuery(bool asSplitQuery = true)
    {
        AsSplitQuery = asSplitQuery;
    }
}
