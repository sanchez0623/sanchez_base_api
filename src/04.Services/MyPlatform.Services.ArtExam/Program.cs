using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MyPlatform.Services.ArtExam;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ArtExamOptions>(builder.Configuration.GetSection(ArtExamOptions.SectionName));
builder.Services.AddHttpClient(nameof(RemoteJsonQuestionBankSource));

builder.Services.AddSingleton<IQuestionBankSource>(sp =>
{
    var options = sp.GetRequiredService<IOptions<ArtExamOptions>>().Value;
    var environment = sp.GetRequiredService<IHostEnvironment>();
    var seedPath = Path.Combine(environment.ContentRootPath, options.QuestionBankSeedFile);

    return new SeedFileQuestionBankSource("本地艺术史种子题库", seedPath);
});

var remoteSources = builder.Configuration.GetSection($"{ArtExamOptions.SectionName}:RemoteQuestionBankSources").Get<string[]>() ?? [];
foreach (var remoteSource in remoteSources.Where(static source => !string.IsNullOrWhiteSpace(source)).Distinct(StringComparer.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IQuestionBankSource>(sp =>
        new RemoteJsonQuestionBankSource(
            remoteSource,
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(RemoteJsonQuestionBankSource))));
}

builder.Services.AddSingleton<QuestionBankService>();
builder.Services.AddSingleton<ExamPaperService>();
builder.Services.AddHostedService<ArtExamBootstrapHostedService>();
builder.Services.AddHostedService<QuestionBankRefreshHostedService>();
builder.Services.AddHostedService<ExamGenerationHostedService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Art Exam MVP API",
        Version = "v1",
        Description = "面向西方艺术史 / 世界艺术史自测的最小 MVP，支持题库刷新、定时出题与答案查看。"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Art Exam MVP API v1");
    });
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/dashboard", (QuestionBankService questionBankService, ExamPaperService examPaperService, IOptions<ArtExamOptions> optionsAccessor) =>
{
    var options = optionsAccessor.Value;
    var currentExam = examPaperService.GetCurrentExam();
    var refresh = questionBankService.GetLastRefresh();

    return Results.Ok(new ArtExamDashboard
    {
        Title = "海外艺术史题库自测 MVP",
        BankName = questionBankService.GetBankName(),
        Disclaimer = questionBankService.GetDisclaimer(),
        TotalQuestions = questionBankService.GetQuestions().Count,
        LastQuestionBankRefreshAt = refresh?.RefreshedAt,
        AppliedSources = refresh?.AppliedSources ?? [],
        RefreshWarnings = refresh?.RefreshWarnings ?? [],
        CurrentExam = currentExam,
        DefaultQuestionCount = options.DefaultQuestionCount,
        QuestionBankRefreshIntervalMinutes = options.QuestionBankRefreshIntervalMinutes,
        AutoGenerateIntervalMinutes = options.AutoGenerateIntervalMinutes,
        RemoteQuestionBankSources = options.RemoteQuestionBankSources
    });
});

app.MapGet("/api/question-bank/questions", (QuestionBankService questionBankService) => Results.Ok(questionBankService.GetQuestions()));

app.MapPost("/api/question-bank/refresh", async (QuestionBankService questionBankService, CancellationToken cancellationToken) =>
{
    var result = await questionBankService.RefreshAsync(cancellationToken);
    return Results.Ok(result);
});

app.MapGet("/api/exams/current", (ExamPaperService examPaperService) =>
{
    var currentExam = examPaperService.GetCurrentExam();
    return currentExam is null
        ? Results.NotFound(new { message = "当前还没有试卷，请先手动生成或等待自动出题。" })
        : Results.Ok(currentExam);
});

app.MapPost("/api/exams/generate", async (GenerateExamRequest? request, ExamPaperService examPaperService, CancellationToken cancellationToken) =>
{
    var exam = await examPaperService.GenerateAsync(request?.QuestionCount, "manual", cancellationToken);
    return Results.Ok(exam);
});

app.MapGet("/health", (QuestionBankService questionBankService, ExamPaperService examPaperService) => Results.Ok(new
{
    status = "ok",
    questionCount = questionBankService.GetQuestions().Count,
    hasCurrentExam = examPaperService.GetCurrentExam() is not null
}));

app.Run();

public partial class Program;
