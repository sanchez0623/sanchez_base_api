using Microsoft.Extensions.Options;

namespace MyPlatform.Shared.Utils.Generators;

/// <summary>
/// Snowflake ID generator configuration options.
/// </summary>
public class SnowflakeIdOptions
{
    /// <summary>
    /// Gets or sets the worker identifier (0-31).
    /// </summary>
    public int WorkerId { get; set; }

    /// <summary>
    /// Gets or sets the data center identifier (0-31).
    /// </summary>
    public int DatacenterId { get; set; }

    /// <summary>
    /// Gets or sets the epoch start time (default: 2020-01-01).
    /// </summary>
    public DateTime Epoch { get; set; } = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}

/// <summary>
/// Snowflake ID generator for distributed unique ID generation.
/// Generates 64-bit unique IDs based on timestamp, datacenter, worker, and sequence.
/// </summary>
public class SnowflakeIdGenerator
{
    private const int WorkerIdBits = 5;
    private const int DatacenterIdBits = 5;
    private const int SequenceBits = 12;

    private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
    private const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);

    private const int WorkerIdShift = SequenceBits;
    private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
    private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

    private const long SequenceMask = -1L ^ (-1L << SequenceBits);

    private readonly long _workerId;
    private readonly long _datacenterId;
    private readonly long _epoch;
    private readonly object _lock = new();

    private long _sequence;
    private long _lastTimestamp = -1L;

    public SnowflakeIdGenerator(IOptions<SnowflakeIdOptions> options)
        : this(options.Value.WorkerId, options.Value.DatacenterId, options.Value.Epoch)
    {
    }

    public SnowflakeIdGenerator(int workerId, int datacenterId, DateTime? epoch = null)
    {
        if (workerId > MaxWorkerId || workerId < 0)
        {
            throw new ArgumentException($"Worker ID must be between 0 and {MaxWorkerId}", nameof(workerId));
        }

        if (datacenterId > MaxDatacenterId || datacenterId < 0)
        {
            throw new ArgumentException($"Datacenter ID must be between 0 and {MaxDatacenterId}", nameof(datacenterId));
        }

        _workerId = workerId;
        _datacenterId = datacenterId;
        _epoch = (epoch ?? new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks / TimeSpan.TicksPerMillisecond;
    }

    /// <summary>
    /// Generates a new unique Snowflake ID.
    /// </summary>
    /// <returns>A unique 64-bit identifier.</returns>
    public long NextId()
    {
        lock (_lock)
        {
            var timestamp = GetCurrentTimestamp();

            if (timestamp < _lastTimestamp)
            {
                throw new InvalidOperationException($"Clock moved backwards. Refusing to generate ID for {_lastTimestamp - timestamp} milliseconds");
            }

            if (timestamp == _lastTimestamp)
            {
                _sequence = (_sequence + 1) & SequenceMask;
                if (_sequence == 0)
                {
                    timestamp = WaitForNextMillis(_lastTimestamp);
                }
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;

            return ((timestamp - _epoch) << TimestampLeftShift) |
                   (_datacenterId << DatacenterIdShift) |
                   (_workerId << WorkerIdShift) |
                   _sequence;
        }
    }

    /// <summary>
    /// Parses a Snowflake ID into its components.
    /// </summary>
    /// <param name="id">The Snowflake ID to parse.</param>
    /// <returns>A tuple containing timestamp, datacenter ID, worker ID, and sequence.</returns>
    public (DateTime Timestamp, long DatacenterId, long WorkerId, long Sequence) Parse(long id)
    {
        var timestamp = ((id >> TimestampLeftShift) + _epoch);
        var datacenterId = (id >> DatacenterIdShift) & MaxDatacenterId;
        var workerId = (id >> WorkerIdShift) & MaxWorkerId;
        var sequence = id & SequenceMask;

        var dateTime = new DateTime(timestamp * TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);

        return (dateTime, datacenterId, workerId, sequence);
    }

    private long GetCurrentTimestamp()
    {
        return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
    }

    private long WaitForNextMillis(long lastTimestamp)
    {
        var timestamp = GetCurrentTimestamp();
        while (timestamp <= lastTimestamp)
        {
            timestamp = GetCurrentTimestamp();
        }
        return timestamp;
    }
}
