
using System;
using System.Collections.Generic;
using MyPlatform.SDK.IdGenerator.Abstractions;
using MyPlatform.SDK.IdGenerator.Segment;
using MyPlatform.SDK.IdGenerator.Snowflake;

namespace MyPlatform.Services.IdDemo
{
    /// <summary>
    /// ID Generation Demo Service / ID 生成演示服务
    /// Demonstrates best practices for generating Order IDs, SKU Codes, and Payment Serials.
    /// 展示生成订单号、SKU编码和支付流水号的最佳实践。
    /// </summary>
    public class IdGeneratorDemoService
    {
        private readonly IIdGenerator _snowflakeGenerator;
        private readonly SegmentIdGenerator _skuSegmentGenerator;

        /// <summary>
        /// Constructor injection / 构造函数注入
        /// </summary>
        /// <param name="snowflakeGenerator">Registered via DI as Singleton / 通过DI注册单例</param>
        /// <param name="skuSegmentGenerator">Manually instantiated or named injection for specific business tags / 手动实例化或命名注入特定业务Tag</param>
        public IdGeneratorDemoService(IIdGenerator snowflakeGenerator, ISegmentRepository segmentRepository)
        {
            _snowflakeGenerator = snowflakeGenerator;
            
            // For SKU, we want short, incrementing numbers (e.g., 10001, 10002).
            // 对于SKU，我们想要简短、递增的数字（如 10001, 10002）。
            // We use the Segment (Leaf) algorithm with the tag "sku_code".
            // 我们使用 "sku_code" 业务Tag的号段（Leaf）算法。
            _skuSegmentGenerator = new SegmentIdGenerator(segmentRepository, "sku_code");
        }

        /// <summary>
        /// 1. Generate Order ID / 生成订单号
        /// Requirement: High QPS, Globally Unique, Time-ordered.
        /// 要求：高并发、全局唯一、时间有序。
        /// </summary>
        public long GenerateOrderId()
        {
            // Snowflake provides standard 64-bit Long IDs.
            // 雪花算法提供标准的 64位 Long ID。
            // Example: 1293849201923
            return _snowflakeGenerator.NextId();
        }

        /// <summary>
        /// 2. Generate SKU Code / 生成 SKU 编码
        /// Requirement: Human readable, Short, Monotonic (for internal management).
        /// 要求：人类可读、简短、单调递增（用于内部管理）。
        /// </summary>
        public string GenerateSkuCode(string categoryPrefix)
        {
            // Get next monotonic ID. 
            // 获取下一个单调递增ID。
            // Example: 10005
            long numericId = _skuSegmentGenerator.NextId();
            
            // Combine with prefix.
            // 结合前缀。
            // Example: "elec-10005"
            return $"{categoryPrefix}-{numericId}";
        }

        /// <summary>
        /// 3. Generate Payment Transaction Serial / 生成支付流水号
        /// Requirement: Unique, Traceable, often contains date info.
        /// 要求：唯一、可追溯，通常包含日期信息。
        /// </summary>
        public string GeneratePaymentSerial()
        {
            // Use Snowflake for the unique core.
            // 使用雪花算法作为唯一核心。
            long uniqueId = _snowflakeGenerator.NextId();
            
            // Format with explicit date for finance reconciliation convenience.
            // 格式化为包含显式日期，便于财务对账。
            // Format: yyyyMMdd + SnowflakeId
            // 格式：yyyyMMdd + 雪花ID
            // Example: 202512061293849201923
            return $"{DateTime.UtcNow:yyyyMMdd}{uniqueId}";
        }
    }
}
