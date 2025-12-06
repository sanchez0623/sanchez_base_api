using Microsoft.AspNetCore.Mvc;
using MyPlatform.Services.Export.Application.Dtos;
using MyPlatform.Services.Export.Application.Services;

namespace MyPlatform.Services.Export.Controllers;

/// <summary>
/// 导出作业控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly ExportAppService _exportService;

    public ExportController(ExportAppService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>
    /// 创建导出作业
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateExportJobResponse>> CreateJob(
        [FromBody] CreateExportJobRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _exportService.CreateJobAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// 获取作业状态
    /// </summary>
    [HttpGet("{jobId:guid}/status")]
    public async Task<ActionResult<ExportJobStatusDto>> GetJobStatus(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var status = await _exportService.GetJobStatusAsync(jobId, cancellationToken);
        if (status == null)
        {
            return NotFound(new { message = "作业不存在" });
        }
        return Ok(status);
    }
}
