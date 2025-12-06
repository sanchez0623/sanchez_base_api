
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyPlatform.SDK.IdGenerator.Abstractions;

namespace MyPlatform.SDK.IdGenerator.Segment
{
    internal class Segment
    {
        public long CurrentId { get; set; }
        public long MaxId { get; set; }
        public int Step { get; set; }
        
        public Segment(LeafAlloc alloc)
        {
            // We retrieved a range. The 'alloc.MaxId' is the *end* of the range we just reserved.
            // So the range is (alloc.MaxId - alloc.Step + 1) to alloc.MaxId.
            // CurrentId starts at the beginning of this range.
            MaxId = alloc.MaxId;
            Step = alloc.Step;
            CurrentId = alloc.MaxId - alloc.Step; 
        }

        public long GetRemaining()
        {
            return MaxId - CurrentId;
        }
    }

    internal class SegmentBuffer
    {
        public string Key { get; }
        public Segment CurrentSegment { get; set; }     // Volatile not strictly needed if accessed under lock
        public Segment NextSegment { get; set; }
        public bool IsNextReady { get; set; }
        public bool IsLoadingNext { get; set; }
        public object Lock { get; } = new object();
        public int Step { get; set; }
        public int MinStep { get; set; }
        public long UpdateTimestamp { get; set; }

        public SegmentBuffer(string key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// Distributed ID generator based on Meituan Leaf Segment algorithm.
    /// Uses database to allocate ranges (segments) of IDs and caches them in memory.
    /// Implements double-buffering to pre-fetch the next segment to minimize latency.
    /// </summary>
    public class SegmentIdGenerator : IIdGenerator
    {
        private readonly ISegmentRepository _repository;
        private readonly string _bizTag;
        private readonly SegmentBuffer _buffer;
        private readonly object _mainLock = new object();

        private bool _initOk = false;

        public SegmentIdGenerator(ISegmentRepository repository, string bizTag)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _bizTag = bizTag ?? throw new ArgumentNullException(nameof(bizTag));
            
            _buffer = new SegmentBuffer(bizTag);
            
            // Eager load the first segment
            if (!Init())
            {
                 throw new Exception($"Init segment ID generator for tag {_bizTag} failed.");
            }
        }

        public long NextId()
        {
            if (!_initOk)
            {
                throw new Exception($"Segment ID generator for tag {_bizTag} is not initialized properly.");
            }

            while (true)
            {
                lock (_mainLock)
                {
                    var userNextId = ProcessNextId();
                    if (userNextId > 0)
                    {
                        return userNextId;
                    }
                }
                
                // If we are here, it means we couldn't get an ID. 
                // Wait a bit (spin wait or sleep) - usually implies DB is down or massive contention.
                // For simplicity, simple wait.
                Thread.Sleep(10);
            }
        }

        private long ProcessNextId()
        {
            var segment = _buffer.CurrentSegment;
            if (!_initOk || segment == null)
            {
                throw new Exception($"Segment ID generator for tag {_bizTag} is not initialized.");
            }

            // Check if we need to pre-load the next buffer using the 10%-90% rule (Leaf logic usually 30-40%?)
            // If remaining < 0.9 * Step (wait, that implies we just started).
            // Actually, if consumed > 10% or just check remaining.
            // Let's use: if remaining < 0.4 * Step, trigger load.
            long remaining = segment.GetRemaining();
            if (remaining < 0.9 * segment.Step && !_buffer.IsNextReady && !_buffer.IsLoadingNext)
            {
                _buffer.IsLoadingNext = true;
                // Async load next segment
                Task.Run(() => LoadNextSegment());
            }

            // Try to get ID from current segment
            if (segment.CurrentId < segment.MaxId)
            {
                 segment.CurrentId++;
                 return segment.CurrentId;
            }

            // Current segment exhausted. Check if next is ready.
            if (_buffer.IsNextReady)
            {
                _buffer.CurrentSegment = _buffer.NextSegment;
                _buffer.NextSegment = null;
                _buffer.IsNextReady = false;
                
                // Return first ID of the new segment
                var newSeg = _buffer.CurrentSegment;
                newSeg.CurrentId++;
                return newSeg.CurrentId;
            }

            // Next not ready, exhausted current. 
            // This is the blocking case, we MUST wait for the background thread or do it ourselves?
            // In strict double-buffer logic, we might block here.
            // Or try to load synchronously if the async one hasn't finished (race condition carefully).
            // For now, return -1 to indicate "wait loop".
            return -1;
        }

        private void LoadNextSegment()
        {
            try
            {
                var alloc = UpdateLeafAlloc(_bizTag);
                var newSegment = new Segment(alloc);
                
                lock (_mainLock) // Or a fines-grained lock for buffer.Lock
                {
                    _buffer.NextSegment = newSegment;
                    _buffer.IsNextReady = true;
                    _buffer.IsLoadingNext = false;
                }
            }
            catch (Exception ex)
            {
                // Log failure
                _buffer.IsLoadingNext = false;
                Console.WriteLine($"[SegmentIdGenerator] Error loading next segment: {ex.Message}");
            }
        }

        private bool Init()
        {
            try 
            {
                var alloc = UpdateLeafAlloc(_bizTag);
                _buffer.CurrentSegment = new Segment(alloc);
                _buffer.Step = alloc.Step;
                _buffer.MinStep = alloc.Step; // simple init
                _initOk = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SegmentIdGenerator] Init error: {ex.Message}");
                return false;
            }
        }

        private LeafAlloc UpdateLeafAlloc(string key)
        {
            // Simple retry logic
            for (int i = 0; i < 3; i++)
            {
                var alloc = _repository.GetLeafAlloc(key);
                if (alloc == null) throw new Exception($"LeafAlloc for {key} not found");

                var newMax = alloc.MaxId + alloc.Step;
                if (_repository.UpdateMaxId(key, newMax))
                {
                    // Success, return the range we just reserved: [alloc.MaxId + 1, newMax]
                    // The 'alloc' object currently has the OLD maxId.
                    // But we want to return a LeafAlloc that represents the reservation we just made.
                    // The Segment constructor expects an object where MaxId is the *end* of the range.
                    return new LeafAlloc
                    {
                        Key = key,
                        MaxId = newMax,
                        Step = alloc.Step,
                        UpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                }
            }
            throw new Exception($"Failed to update MaxId for {key} after retries");
        }

        public IEnumerable<long> NextIds(int count)
        {
            // Basic implementation: loop.
            // Optimized implementation could grab a chunk from segment directly.
            var list = new List<long>(count);
            for(int i=0; i<count; i++)
            {
                list.Add(NextId());
            }
            return list;
        }
    }
}
