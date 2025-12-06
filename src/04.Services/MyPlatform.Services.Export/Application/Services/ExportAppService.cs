using Microsoft.Extensions.Logging;
using MyPlatform.SDK.DataExchange.Jobs;
using MyPlatform.SDK.EventBus.Abstractions;
using MyPlatform.Services.Export.Application.Dtos;
using MyPlatform.Services.Export.Domain.Events;

namespace MyPlatform.Services.Export.Application.Services;

/// <summary>
/// 导出作业应用服务
/// 负责：创建作业、发布事件、查询状态
/// </summary>
public class ExportAppService
{
    private readonly ILogger<ExportAppService> _logger;
    private readonly IDataExchangeJobRepository _jobRepository;
    private readonly IEventPublisher _eventPublisher;

    public ExportAppService(
        ILogger<ExportAppService> logger,
        IDataExchangeJobRepository jobRepository,
        IEventPublisher eventPublisher)
    {
        _logger = logger;
        _jobRepository = jobRepository;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// 创建导出作业
    /// 非阻塞：发布事件后立即返回
    /// </summary>
    public async Task<CreateExportJobResponse> CreateJobAsync(
        CreateExportJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var job = new DataExchangeJob
        {
            Type = DataExchangeJobType.Export,
            Status = DataExchangeJobStatus.Pending,
            InitiatedBy = request.UserId,
            Version = 1
        };

        await _jobRepository.CreateAsync(job, cancellationToken);

        await _eventPublisher.PublishAsync(new ExportJobCreatedEvent
        {
            JobId = job.Id,
            ExpectedVersion = job.Version
        }, cancellationToken);

        _logger.LogInformation("导出作业已创建: {JobId}", job.Id);

        return new CreateExportJobResponse
        {
            JobId = job.Id,
            Status = job.Status.ToString(),
            Message = "作业已创建，请使用状态接口跟踪进度"
        };
    }

    /// <summary>
    /// 获取作业状态
    /// </summary>
    public async Task<ExportJobStatusDto?> GetJobStatusAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null) return null;

        return MapToDto(job);
    }

    private static ExportJobStatusDto MapToDto(DataExchangeJob job)
    {
        return new ExportJobStatusDto
        {
            JobId = job.Id,
            Status = job.Status.ToString(),
            ProgressPercent = job.TotalCount > 0
                ? (int)(job.ProcessedCount * 100.0 / job.TotalCount.Value)
                : null,
            DownloadUrl = job.ResultFileUrl,
            ErrorMessage = job.ErrorMessage,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt
        };
    }
}
