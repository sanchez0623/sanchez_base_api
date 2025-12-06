
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyPlatform.SDK.IdGenerator.Abstractions;
using MyPlatform.SDK.IdGenerator.Snowflake;
using MyPlatform.SDK.IdGenerator.Segment;

namespace MyPlatform.SDK.IdGenerator.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Snowflake ID Generator services to the container.
        /// </summary>
        public static IServiceCollection AddSnowflakeIdGenerator(this IServiceCollection services, long workerId)
        {
            services.TryAddSingleton<IWorkerIdProvider>(new StaticWorkerIdProvider(workerId));
            services.TryAddSingleton<IIdGenerator, SnowflakeIdGenerator>();
            // Also register as concrete type if needed
            services.TryAddSingleton<SnowflakeIdGenerator>();
            return services;
        }

        // We can add AddSegmentIdGenerator later when we have a concrete repo implementation.
    }

    /// <summary>
    /// Simple static worker provider for testing/single-node.
    /// </summary>
    public class StaticWorkerIdProvider : IWorkerIdProvider
    {
        private readonly long _workerId;
        public StaticWorkerIdProvider(long workerId) => _workerId = workerId;
        public long GetWorkerId() => _workerId;
    }
}
