
using System.Collections.Generic;

namespace MyPlatform.SDK.IdGenerator.Abstractions
{
    /// <summary>
    /// Defines the contract for a distributed unique ID generator.
    /// </summary>
    public interface IIdGenerator
    {
        /// <summary>
        /// Generates the next unique ID.
        /// </summary>
        /// <returns>A unique long ID.</returns>
        long NextId();

        /// <summary>
        /// Generates a batch of unique IDs.
        /// </summary>
        /// <param name="count">Number of IDs to generate.</param>
        /// <returns>A list of unique IDs.</returns>
        IEnumerable<long> NextIds(int count);
    }
}
