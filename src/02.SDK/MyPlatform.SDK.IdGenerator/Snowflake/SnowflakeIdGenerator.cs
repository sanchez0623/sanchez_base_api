
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MyPlatform.SDK.IdGenerator.Abstractions;

namespace MyPlatform.SDK.IdGenerator.Snowflake
{
    /// <summary>
    /// Distributed ID generator based on Twitter's Snowflake algorithm.
    /// Structure: 1 bit sign | 41 bits timestamp | 10 bits worker ID | 12 bits sequence.
    /// </summary>
    public class SnowflakeIdGenerator : IIdGenerator
    {
        // Custom Epoch (November 4, 2010 01:42:54.657 GMT) - Twitter Snowflake epoch
        // You can adjust this to a recent date to extend the lifespan.
        private const long Twepoch = 1288834974657L;

        // Bit lengths
        private const int WorkerIdBits = 10;
        private const int SequenceBits = 12;

        // Max values
        private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
        private const long SequenceMask = -1L ^ (-1L << SequenceBits);

        // Shifts
        private const int WorkerIdShift = SequenceBits;
        private const int TimestampLeftShift = SequenceBits + WorkerIdBits;

        private readonly long _workerId;
        private long _sequence = 0L;
        private long _lastTimestamp = -1L;
        
        private readonly object _lock = new object();

        public SnowflakeIdGenerator(IWorkerIdProvider workerIdProvider)
        {
            if (workerIdProvider == null) throw new ArgumentNullException(nameof(workerIdProvider));

            long workerId = workerIdProvider.GetWorkerId();
            if (workerId > MaxWorkerId || workerId < 0)
            {
                throw new ArgumentException($"Worker Id can't be greater than {MaxWorkerId} or less than 0");
            }
            _workerId = workerId;
        }

        public long NextId()
        {
            lock (_lock)
            {
                var timestamp = TimeGen();

                if (timestamp < _lastTimestamp)
                {
                    // Clock moved backwards, refuse to generate id
                    throw new Exception($"Clock moved backwards. Refusing to generate id for {_lastTimestamp - timestamp} milliseconds");
                }

                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & SequenceMask;
                    if (_sequence == 0)
                    {
                        // Timestamp overflow, wait for next millisecond
                        timestamp = TilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0L;
                }

                _lastTimestamp = timestamp;

                return ((timestamp - Twepoch) << TimestampLeftShift) |
                       (_workerId << WorkerIdShift) |
                       _sequence;
            }
        }

        public IEnumerable<long> NextIds(int count)
        {
            var ids = new List<long>(count);
            for (int i = 0; i < count; i++)
            {
                ids.Add(NextId());
            }
            return ids;
        }

        private long TilNextMillis(long lastTimestamp)
        {
            var timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }
            return timestamp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long TimeGen()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
