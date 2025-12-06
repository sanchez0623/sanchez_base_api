
using System;

namespace MyPlatform.SDK.DataExchange.Jobs
{
    /// <summary>
    /// Type of data exchange job.
    /// 数据交换作业类型。
    /// </summary>
    public enum DataExchangeJobType
    {
        Import,
        Export
    }

    /// <summary>
    /// Status of a data exchange job.
    /// 数据交换作业状态。
    /// </summary>
    public enum DataExchangeJobStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }

    /// <summary>
    /// Represents an async data exchange job for large file processing.
    /// 表示用于大文件处理的异步数据交换作业。
    /// </summary>
    public class DataExchangeJob
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Job type (Import or Export).
        /// 作业类型（导入或导出）。
        /// </summary>
        public DataExchangeJobType Type { get; set; }
        
        /// <summary>
        /// Current status of the job.
        /// 作业的当前状态。
        /// </summary>
        public DataExchangeJobStatus Status { get; set; } = DataExchangeJobStatus.Pending;
        
        /// <summary>
        /// For import: source file URL; For export: result file URL after completion.
        /// 导入时：源文件 URL；导出时：完成后的结果文件 URL。
        /// </summary>
        public string? SourceFileUrl { get; set; }
        
        /// <summary>
        /// Result file URL (for exports, after completion).
        /// 结果文件 URL（导出完成后）。
        /// </summary>
        public string? ResultFileUrl { get; set; }
        
        /// <summary>
        /// Error message if job failed.
        /// 作业失败时的错误信息。
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Number of records processed.
        /// 已处理的记录数。
        /// </summary>
        public int ProcessedCount { get; set; }
        
        /// <summary>
        /// Total records (if known).
        /// 总记录数（如果已知）。
        /// </summary>
        public int? TotalCount { get; set; }
        
        /// <summary>
        /// User or tenant who initiated the job.
        /// 发起作业的用户或租户。
        /// </summary>
        public string? InitiatedBy { get; set; }
        
        /// <summary>
        /// Version for optimistic concurrency control.
        /// 用于乐观并发控制的版本号。
        /// Incremented on each status change. Used to prevent race conditions in distributed environments.
        /// 每次状态变更时递增。用于防止分布式环境中的竞态条件。
        /// </summary>
        public int Version { get; set; } = 1;
        
        /// <summary>
        /// Job creation time.
        /// 作业创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Job completion time.
        /// 作业完成时间。
        /// </summary>
        public DateTime? CompletedAt { get; set; }
    }
}
