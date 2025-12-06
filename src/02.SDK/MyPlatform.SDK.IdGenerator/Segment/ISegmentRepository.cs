
using System.Threading.Tasks;

namespace MyPlatform.SDK.IdGenerator.Segment
{
    public class LeafAlloc
    {
        public string Key { get; set; }
        public long MaxId { get; set; }
        public int Step { get; set; }
        public long UpdateTime { get; set; } // Optional: for debugging or optimistic locking logic
    }

    /// <summary>
    /// Repository interface for Leaf Segment mode.
    /// Consumers implement this using their preferred data access technology (EF Core, Dapper, etc.).
    /// </summary>
    public interface ISegmentRepository
    {
        /// <summary>
        /// Gets the current allocation information for a given business tag.
        /// </summary>
        /// <param name="key">Business tag (e.g. "order_id")</param>
        /// <returns>LeafAlloc object containing current MaxId and Step.</returns>
        LeafAlloc GetLeafAlloc(string key);

        /// <summary>
        /// Atomically updates the MaxId for a given business tag.
        /// Equivalent SQL: UPDATE table SET max_id = max_id + step WHERE key = @key AND max_id = @oldMaxId
        /// </summary>
        /// <param name="key">Business tag</param>
        /// <param name="maxId">New max ID</param>
        /// <returns>True if update succeeded.</returns>
        bool UpdateMaxId(string key, long maxId);
        
        /// <summary>
        /// Same as GetLeafAlloc but async.
        /// </summary>
        Task<LeafAlloc> GetLeafAllocAsync(string key);

        /// <summary>
        /// Same as UpdateMaxId but async.
        /// </summary>
        Task<bool> UpdateMaxIdAsync(string key, long maxId);

        // Note: For simplicity in the generator, we might mostly rely on synchronous methods inside locks, 
        // or carefully manage async in the buffer loading.
    }
}
