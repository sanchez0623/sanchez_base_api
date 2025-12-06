using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MyPlatform.SDK.DataExchange.Excel;
using MyPlatform.SDK.DataExchange.Jobs;
using MyPlatform.SDK.Storage.Abstractions;
using MyPlatform.SDK.Storage.Models;

namespace MyPlatform.Services.Export.Infrastructure.Workers;

/// <summary>
/// 导出作业后台Worker
/// 负责：认领作业、生成文件、上传存储、更新状态
/// </summary>
public class ExportJobWorker
{
    private readonly ILogger<ExportJobWorker> _logger;
    private readonly IDataExchangeJobRepository _jobRepository;
    private readonly IStorageService _storageService;

    public ExportJobWorker(
        ILogger<ExportJobWorker> logger,
        IDataExchangeJobRepository jobRepository,
        IStorageService storageService)
    {
        _logger = logger;
        _jobRepository = jobRepository;
        _storageService = storageService;
    }

    /// <summary>
    /// 处理导出作业
    /// 由消息队列消费者调用
    /// </summary>
    public async Task ProcessAsync(
        Guid jobId,
        int expectedVersion,
        CancellationToken cancellationToken = default)
    {
        // 原子认领（防止K8s多Pod重复处理）
        var claimed = await _jobRepository.TryClaimJobAsync(jobId, expectedVersion, cancellationToken);
        if (!claimed)
        {
            _logger.LogInformation("作业 {JobId} 已被其他Worker认领", jobId);
            return;
        }

        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null) return;

        _logger.LogInformation("开始处理作业: {JobId}", jobId);

        try
        {
            // 生成Excel
            var data = FetchDataAsync(job.InitiatedBy, cancellationToken);
            var writer = new ExcelDataWriter<OrderExportDto>();
            await using var stream = await writer.WriteAsync(data, cancellationToken);

            // 上传到OSS
            var fileName = $"exports/{job.Id:N}.xlsx";
            var result = await _storageService.UploadAsync(stream, fileName, new UploadOptions
            {
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            }, cancellationToken);

            // 更新作业状态
            job.ResultFileUrl = result.PublicUrl ?? await _storageService.GetPresignedDownloadUrlAsync(result.Key);
            job.Status = DataExchangeJobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            await _jobRepository.UpdateAsync(job, cancellationToken);

            _logger.LogInformation("作业完成: {JobId}, URL: {Url}", jobId, job.ResultFileUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "作业失败: {JobId}", jobId);

            job.Status = DataExchangeJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            await _jobRepository.UpdateAsync(job, cancellationToken);

            throw; // 触发消息队列的死信机制
        }
    }

    private async IAsyncEnumerable<OrderExportDto> FetchDataAsync(
        string? userId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 生产环境：替换为实际的数据库查询
        for (int i = 1; i <= 1000; i++)
        {
            if (cancellationToken.IsCancellationRequested) yield break;
            yield return new OrderExportDto
            {
                OrderId = 1000000 + i,
                CustomerName = $"Customer {i}",
                TotalAmount = 99.99m * (i % 10 + 1),
                Status = i % 3 == 0 ? "Completed" : "Pending",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            };
            if (i % 100 == 0) await Task.Delay(5, cancellationToken);
        }
    }
}

/// <summary>
/// 订单导出DTO（Worker内部使用）
/// </summary>
public class OrderExportDto
{
    public long OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
