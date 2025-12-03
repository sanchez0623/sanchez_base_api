using MyPlatform.Services.Worker.Models;
using MyPlatform.Services.Worker.Models.Requests;

namespace MyPlatform.Services.Worker.Services;

/// <summary>
/// 任务管理服务接口
/// </summary>
public interface IJobManagementService
{
    /// <summary>
    /// 获取所有任务列表
    /// </summary>
    /// <returns>任务信息列表</returns>
    Task<IEnumerable<JobInfo>> GetAllJobsAsync();

    /// <summary>
    /// 获取指定任务详情
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="groupName">任务组名（可选，默认为 DEFAULT）</param>
    /// <returns>任务信息，如果不存在返回 null</returns>
    Task<JobInfo?> GetJobAsync(string jobName, string? groupName = null);

    /// <summary>
    /// 添加新任务
    /// </summary>
    /// <param name="request">添加任务请求</param>
    /// <returns>是否成功</returns>
    Task<bool> AddJobAsync(AddJobRequest request);

    /// <summary>
    /// 删除任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="groupName">任务组名（可选，默认为 DEFAULT）</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteJobAsync(string jobName, string? groupName = null);

    /// <summary>
    /// 暂停任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="groupName">任务组名（可选，默认为 DEFAULT）</param>
    /// <returns>是否成功</returns>
    Task<bool> PauseJobAsync(string jobName, string? groupName = null);

    /// <summary>
    /// 恢复任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="groupName">任务组名（可选，默认为 DEFAULT）</param>
    /// <returns>是否成功</returns>
    Task<bool> ResumeJobAsync(string jobName, string? groupName = null);

    /// <summary>
    /// 立即触发任务执行
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="groupName">任务组名（可选，默认为 DEFAULT）</param>
    /// <returns>是否成功</returns>
    Task<bool> TriggerJobAsync(string jobName, string? groupName = null);

    /// <summary>
    /// 更新任务调度
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="request">更新请求</param>
    /// <param name="groupName">任务组名（可选，默认为 DEFAULT）</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateScheduleAsync(string jobName, UpdateScheduleRequest request, string? groupName = null);

    /// <summary>
    /// 获取所有任务组
    /// </summary>
    /// <returns>任务组名称列表</returns>
    Task<IEnumerable<string>> GetAllGroupsAsync();
}
