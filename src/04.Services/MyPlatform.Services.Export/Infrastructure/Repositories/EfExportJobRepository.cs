using Microsoft.EntityFrameworkCore;
using MyPlatform.SDK.DataExchange.Jobs;
using MyPlatform.Services.Export.Infrastructure.Data;

namespace MyPlatform.Services.Export.Infrastructure.Repositories;

/// <summary>
/// EF Core实现的导出作业仓储
/// </summary>
public class EfExportJobRepository : IDataExchangeJobRepository
{
    private readonly ExportDbContext _dbContext;

    public EfExportJobRepository(ExportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DataExchangeJob> CreateAsync(DataExchangeJob job, CancellationToken cancellationToken = default)
    {
        _dbContext.ExportJobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task<DataExchangeJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ExportJobs.FindAsync([id], cancellationToken);
    }

    public async Task<bool> UpdateAsync(DataExchangeJob job, CancellationToken cancellationToken = default)
    {
        try
        {
            job.Version++;
            _dbContext.ExportJobs.Update(job);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    /// <summary>
    /// 原子认领作业（防止分布式环境重复处理）
    /// </summary>
    public async Task<bool> TryClaimJobAsync(Guid jobId, int expectedVersion, CancellationToken cancellationToken = default)
    {
        // 使用原始SQL确保原子性
        var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(
            @"UPDATE export_jobs 
              SET status = 'Processing', version = version + 1 
              WHERE id = {0} AND status = 'Pending' AND version = {1}",
            [jobId, expectedVersion],
            cancellationToken);

        return rowsAffected > 0;
    }
}
