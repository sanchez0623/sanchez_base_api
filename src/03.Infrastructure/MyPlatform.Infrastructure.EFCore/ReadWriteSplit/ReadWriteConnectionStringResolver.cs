using Microsoft.Extensions.Options;

namespace MyPlatform.Infrastructure.EFCore.ReadWriteSplit;

/// <summary>
/// Implementation of connection string resolver for read-write split scenarios.
/// Supports multiple load balancing strategies for replica selection.
/// </summary>
public class ReadWriteConnectionStringResolver : IConnectionStringResolver
{
    private readonly ReadWriteOptions _options;
    private readonly List<ReplicaInfo> _enabledReplicas;
    private static readonly ThreadLocal<Random> _threadLocalRandom = new(() => new Random(Guid.NewGuid().GetHashCode()));
    private int _roundRobinIndex = -1;
    private int _weightedIndex = 0;
    private int _weightedCurrentWeight = 0;
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadWriteConnectionStringResolver"/> class.
    /// </summary>
    /// <param name="options">The read-write split options.</param>
    public ReadWriteConnectionStringResolver(IOptions<ReadWriteOptions> options)
    {
        _options = options.Value;
        _enabledReplicas = _options.Replicas.Where(r => r.Enabled).ToList();
    }

    /// <inheritdoc />
    public string GetWriteConnectionString()
    {
        return _options.Master.ConnectionString;
    }

    /// <inheritdoc />
    public string GetReadConnectionString()
    {
        if (!_options.Enabled || _enabledReplicas.Count == 0)
        {
            return GetWriteConnectionString();
        }

        var replica = SelectReplica();
        return replica?.ConnectionString ?? GetWriteConnectionString();
    }

    /// <inheritdoc />
    public bool ShouldUseMaster()
    {
        return !_options.Enabled || _enabledReplicas.Count == 0;
    }

    /// <summary>
    /// Selects a replica based on the configured load balance strategy.
    /// </summary>
    /// <returns>The selected replica information.</returns>
    private ReplicaInfo? SelectReplica()
    {
        if (_enabledReplicas.Count == 0)
        {
            return null;
        }

        if (_enabledReplicas.Count == 1)
        {
            return _enabledReplicas[0];
        }

        return _options.LoadBalanceStrategy switch
        {
            LoadBalanceStrategy.RoundRobin => SelectRoundRobin(),
            LoadBalanceStrategy.Random => SelectRandom(),
            LoadBalanceStrategy.WeightedRoundRobin => SelectWeightedRoundRobin(),
            LoadBalanceStrategy.LeastConnections => SelectRoundRobin(), // Fallback to round-robin
            _ => SelectRoundRobin()
        };
    }

    /// <summary>
    /// Selects a replica using round-robin strategy.
    /// </summary>
    private ReplicaInfo SelectRoundRobin()
    {
        lock (_lockObject)
        {
            _roundRobinIndex = (_roundRobinIndex + 1) % _enabledReplicas.Count;
            return _enabledReplicas[_roundRobinIndex];
        }
    }

    /// <summary>
    /// Selects a replica using random strategy.
    /// </summary>
    private ReplicaInfo SelectRandom()
    {
        var random = _threadLocalRandom.Value!;
        var index = random.Next(_enabledReplicas.Count);
        return _enabledReplicas[index];
    }

    /// <summary>
    /// Selects a replica using weighted round-robin strategy.
    /// </summary>
    private ReplicaInfo SelectWeightedRoundRobin()
    {
        lock (_lockObject)
        {
            var maxIterations = _enabledReplicas.Count * GetMaxWeight() + 1;
            var iterations = 0;

            while (iterations < maxIterations)
            {
                iterations++;
                _weightedIndex = (_weightedIndex + 1) % _enabledReplicas.Count;

                if (_weightedIndex == 0)
                {
                    _weightedCurrentWeight--;
                    if (_weightedCurrentWeight <= 0)
                    {
                        _weightedCurrentWeight = GetMaxWeight();
                        if (_weightedCurrentWeight == 0)
                        {
                            return _enabledReplicas[0];
                        }
                    }
                }

                if (_enabledReplicas[_weightedIndex].Weight >= _weightedCurrentWeight)
                {
                    return _enabledReplicas[_weightedIndex];
                }
            }

            // Safety fallback: return first replica if algorithm fails
            return _enabledReplicas[0];
        }
    }

    /// <summary>
    /// Gets the maximum weight among all enabled replicas.
    /// </summary>
    private int GetMaxWeight()
    {
        return _enabledReplicas.Max(r => r.Weight);
    }
}
