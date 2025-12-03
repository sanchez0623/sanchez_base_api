using Microsoft.AspNetCore.Mvc;
using MyPlatform.Services.Worker.Models;
using MyPlatform.Services.Worker.Models.Requests;
using MyPlatform.Services.Worker.Services;

namespace MyPlatform.Services.Worker.Controllers;

/// <summary>
/// 任务管理 API 控制器
/// </summary>
[ApiController]
[Route("api/jobs")]
[Produces("application/json")]
public class JobManagementController : ControllerBase
{
    private readonly IJobManagementService _jobManagementService;
    private readonly IJobExecutionHistoryService _historyService;
    private readonly ILogger<JobManagementController> _logger;

    /// <summary>
    /// 初始化任务管理控制器
    /// </summary>
    /// <param name="jobManagementService">任务管理服务</param>
    /// <param name="historyService">执行历史服务</param>
    /// <param name="logger">日志记录器</param>
    public JobManagementController(
        IJobManagementService jobManagementService,
        IJobExecutionHistoryService historyService,
        ILogger<JobManagementController> logger)
    {
        _jobManagementService = jobManagementService;
        _historyService = historyService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有任务列表
    /// </summary>
    /// <returns>任务信息列表</returns>
    /// <response code="200">成功返回任务列表</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JobInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<JobInfo>>> GetAllJobs()
    {
        _logger.LogInformation("获取所有任务列表");
        var jobs = await _jobManagementService.GetAllJobsAsync();
        return Ok(jobs);
    }

    /// <summary>
    /// 获取单个任务详情
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="group">任务组（可选）</param>
    /// <returns>任务详情</returns>
    /// <response code="200">成功返回任务详情</response>
    /// <response code="404">任务不存在</response>
    [HttpGet("{jobName}")]
    [ProducesResponseType(typeof(JobInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobInfo>> GetJob(string jobName, [FromQuery] string? group = null)
    {
        _logger.LogInformation("获取任务详情: {JobName}", jobName);
        var job = await _jobManagementService.GetJobAsync(jobName, group);
        if (job == null)
        {
            return NotFound(new { Message = $"任务 '{jobName}' 不存在" });
        }
        return Ok(job);
    }

    /// <summary>
    /// 动态添加新任务
    /// </summary>
    /// <param name="request">添加任务请求</param>
    /// <returns>操作结果</returns>
    /// <response code="201">任务创建成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="409">任务已存在</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddJob([FromBody] AddJobRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("添加任务: {JobName}", request.JobName);
        var result = await _jobManagementService.AddJobAsync(request);
        if (!result)
        {
            return Conflict(new { Message = $"添加任务 '{request.JobName}' 失败，任务可能已存在或类型无效" });
        }

        return CreatedAtAction(nameof(GetJob), new { jobName = request.JobName }, new { Message = "任务创建成功" });
    }

    /// <summary>
    /// 删除任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="group">任务组（可选）</param>
    /// <returns>操作结果</returns>
    /// <response code="204">删除成功</response>
    /// <response code="404">任务不存在</response>
    [HttpDelete("{jobName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteJob(string jobName, [FromQuery] string? group = null)
    {
        _logger.LogInformation("删除任务: {JobName}", jobName);
        var result = await _jobManagementService.DeleteJobAsync(jobName, group);
        if (!result)
        {
            return NotFound(new { Message = $"任务 '{jobName}' 不存在" });
        }

        return NoContent();
    }

    /// <summary>
    /// 暂停任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="group">任务组（可选）</param>
    /// <returns>操作结果</returns>
    /// <response code="200">暂停成功</response>
    /// <response code="404">任务不存在</response>
    [HttpPost("{jobName}/pause")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PauseJob(string jobName, [FromQuery] string? group = null)
    {
        _logger.LogInformation("暂停任务: {JobName}", jobName);
        var result = await _jobManagementService.PauseJobAsync(jobName, group);
        if (!result)
        {
            return NotFound(new { Message = $"任务 '{jobName}' 不存在" });
        }

        return Ok(new { Message = $"任务 '{jobName}' 已暂停" });
    }

    /// <summary>
    /// 恢复任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="group">任务组（可选）</param>
    /// <returns>操作结果</returns>
    /// <response code="200">恢复成功</response>
    /// <response code="404">任务不存在</response>
    [HttpPost("{jobName}/resume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResumeJob(string jobName, [FromQuery] string? group = null)
    {
        _logger.LogInformation("恢复任务: {JobName}", jobName);
        var result = await _jobManagementService.ResumeJobAsync(jobName, group);
        if (!result)
        {
            return NotFound(new { Message = $"任务 '{jobName}' 不存在" });
        }

        return Ok(new { Message = $"任务 '{jobName}' 已恢复" });
    }

    /// <summary>
    /// 立即触发执行
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="group">任务组（可选）</param>
    /// <returns>操作结果</returns>
    /// <response code="200">触发成功</response>
    /// <response code="404">任务不存在</response>
    [HttpPost("{jobName}/trigger")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TriggerJob(string jobName, [FromQuery] string? group = null)
    {
        _logger.LogInformation("触发任务: {JobName}", jobName);
        var result = await _jobManagementService.TriggerJobAsync(jobName, group);
        if (!result)
        {
            return NotFound(new { Message = $"任务 '{jobName}' 不存在" });
        }

        return Ok(new { Message = $"任务 '{jobName}' 已触发" });
    }

    /// <summary>
    /// 修改调度（Cron表达式）
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="request">更新请求</param>
    /// <param name="group">任务组（可选）</param>
    /// <returns>操作结果</returns>
    /// <response code="200">更新成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">任务不存在</response>
    [HttpPut("{jobName}/schedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSchedule(string jobName, [FromBody] UpdateScheduleRequest request, [FromQuery] string? group = null)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("更新任务调度: {JobName}, 新 Cron: {CronExpression}", jobName, request.CronExpression);
        var result = await _jobManagementService.UpdateScheduleAsync(jobName, request, group);
        if (!result)
        {
            return NotFound(new { Message = $"任务 '{jobName}' 不存在或更新失败" });
        }

        return Ok(new { Message = $"任务 '{jobName}' 调度已更新" });
    }

    /// <summary>
    /// 获取执行历史
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="group">任务组（可选）</param>
    /// <param name="page">页码（默认 1）</param>
    /// <param name="pageSize">每页大小（默认 20）</param>
    /// <returns>执行历史列表</returns>
    /// <response code="200">成功返回执行历史</response>
    [HttpGet("{jobName}/history")]
    [ProducesResponseType(typeof(IEnumerable<JobExecutionHistory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<JobExecutionHistory>>> GetHistory(
        string jobName, 
        [FromQuery] string? group = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("获取任务执行历史: {JobName}", jobName);
        var history = await _historyService.GetHistoryAsync(jobName, group, page, pageSize);
        return Ok(history);
    }

    /// <summary>
    /// 获取所有任务组
    /// </summary>
    /// <returns>任务组名称列表</returns>
    /// <response code="200">成功返回任务组列表</response>
    [HttpGet("groups")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetGroups()
    {
        _logger.LogInformation("获取所有任务组");
        var groups = await _jobManagementService.GetAllGroupsAsync();
        return Ok(groups);
    }

    /// <summary>
    /// 获取最近执行历史
    /// </summary>
    /// <param name="count">返回数量（默认 50）</param>
    /// <returns>执行历史列表</returns>
    /// <response code="200">成功返回执行历史</response>
    [HttpGet("history/recent")]
    [ProducesResponseType(typeof(IEnumerable<JobExecutionHistory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<JobExecutionHistory>>> GetRecentHistory([FromQuery] int count = 50)
    {
        _logger.LogInformation("获取最近执行历史: {Count} 条", count);
        var history = await _historyService.GetRecentHistoryAsync(count);
        return Ok(history);
    }
}
