using Microsoft.Extensions.Options;

namespace MyPlatform.Services.ArtExam;

public sealed class ArtExamBootstrapHostedService(QuestionBankService questionBankService, ExamPaperService examPaperService, ILogger<ArtExamBootstrapHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await questionBankService.RefreshAsync(cancellationToken);
            await examPaperService.GenerateAsync(null, "startup", cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to initialize the art exam MVP service.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public sealed class QuestionBankRefreshHostedService(QuestionBankService questionBankService, IOptions<ArtExamOptions> optionsAccessor, ILogger<QuestionBankRefreshHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(1, optionsAccessor.Value.QuestionBankRefreshIntervalMinutes));
        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await questionBankService.RefreshAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Automatic question bank refresh failed.");
            }
        }
    }
}

public sealed class ExamGenerationHostedService(ExamPaperService examPaperService, IOptions<ArtExamOptions> optionsAccessor, ILogger<ExamGenerationHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(1, optionsAccessor.Value.AutoGenerateIntervalMinutes));
        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await examPaperService.GenerateAsync(null, "scheduled", stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Automatic exam generation failed.");
            }
        }
    }
}
