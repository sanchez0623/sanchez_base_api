
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyPlatform.SDK.DataExchange.Jobs
{
    /// <summary>
    /// Repository interface for persisting data exchange jobs.
    /// 用于持久化数据交换作业的仓储接口。
    /// 
    /// IMPORTANT: Implementations MUST use optimistic concurrency control.
    /// 重要：实现必须使用乐观并发控制。
    /// </summary>
    public interface IDataExchangeJobRepository
    {
        /// <summary>
        /// Creates a new job record.
        /// 创建新的作业记录。
        /// </summary>
        Task<DataExchangeJob> CreateAsync(DataExchangeJob job, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a job by ID.
        /// 根据 ID 获取作业。
        /// </summary>
        Task<DataExchangeJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing job with optimistic concurrency check.
        /// 使用乐观并发检查更新现有作业。
        /// 
        /// Implementation MUST check Version field:
        /// UPDATE ... WHERE Id = @Id AND Version = @ExpectedVersion
        /// 实现必须检查 Version 字段。
        /// </summary>
        /// <returns>True if updated successfully, False if version conflict (another process modified it).</returns>
        Task<bool> UpdateAsync(DataExchangeJob job, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atomically claims a pending job for processing.
        /// 原子性地认领一个待处理作业进行处理。
        /// 
        /// This is the KEY method for distributed safety.
        /// 这是分布式安全性的关键方法。
        /// 
        /// Implementation should be:
        /// UPDATE DataExchangeJobs 
        /// SET Status = 'Processing', Version = Version + 1, ProcessingStartedAt = @Now
        /// WHERE Id = @Id AND Status = 'Pending' AND Version = @ExpectedVersion
        /// 
        /// If rows affected = 0, another worker already claimed it.
        /// 如果受影响行数 = 0，说明另一个 Worker 已经认领了它。
        /// </summary>
        /// <param name="jobId">Job ID to claim.</param>
        /// <param name="expectedVersion">Expected version (for optimistic locking).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if successfully claimed, False if already claimed by another worker.</returns>
        Task<bool> TryClaimJobAsync(Guid jobId, int expectedVersion, CancellationToken cancellationToken = default);
    }
}
