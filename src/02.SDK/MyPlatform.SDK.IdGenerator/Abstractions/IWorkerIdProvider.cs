
namespace MyPlatform.SDK.IdGenerator.Abstractions
{
    /// <summary>
    /// Provider for obtaining the Worker ID (Machine ID) for Snowflake algorithm.
    /// </summary>
    public interface IWorkerIdProvider
    {
        /// <summary>
        /// Gets the current Worker ID.
        /// </summary>
        /// <returns>A long value representing the worker ID.</returns>
        long GetWorkerId();
    }
}
