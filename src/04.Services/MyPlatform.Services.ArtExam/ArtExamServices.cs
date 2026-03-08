using System.Text.Json;
using Microsoft.Extensions.Options;

namespace MyPlatform.Services.ArtExam;

public interface IQuestionBankSource
{
    string Name { get; }

    Task<ArtQuestionBankDocument> LoadAsync(CancellationToken cancellationToken);
}

public sealed class SeedFileQuestionBankSource(string name, string absolutePath) : IQuestionBankSource
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public string Name => name;

    public async Task<ArtQuestionBankDocument> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException($"题库文件不存在：{absolutePath}");
        }

        await using var stream = File.OpenRead(absolutePath);
        var document = await JsonSerializer.DeserializeAsync<ArtQuestionBankDocument>(stream, SerializerOptions, cancellationToken);
        return document ?? throw new InvalidOperationException($"题库文件 {absolutePath} 为空或格式不正确。");
    }
}

public sealed class RemoteJsonQuestionBankSource(string sourceUrl, HttpClient httpClient) : IQuestionBankSource
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public string Name => sourceUrl;

    public async Task<ArtQuestionBankDocument> LoadAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(sourceUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await JsonSerializer.DeserializeAsync<ArtQuestionBankDocument>(stream, SerializerOptions, cancellationToken);
        return document ?? throw new InvalidOperationException($"远程题库 {sourceUrl} 为空或格式不正确。");
    }
}

public sealed class QuestionBankService(IEnumerable<IQuestionBankSource> sources, ILogger<QuestionBankService> logger)
{
    private const string DefaultDisclaimer = "当前题目为基于海外艺术院校公开课程方向整理的中文练习版，不直接复制受版权保护的原始试卷全文，可用于本地 MVP 演示与自测。";
    private readonly object syncRoot = new();
    private readonly List<IQuestionBankSource> allSources = [.. sources];
    private string bankName = "海外艺术史中文练习题库";
    private string disclaimer = DefaultDisclaimer;
    private List<ArtQuestion> questions = [];
    private QuestionBankRefreshResult? lastRefresh;

    public async Task<QuestionBankRefreshResult> RefreshAsync(CancellationToken cancellationToken)
    {
        var mergedQuestions = new Dictionary<string, ArtQuestion>(StringComparer.OrdinalIgnoreCase);
        var appliedSources = new List<string>();
        var refreshWarnings = new List<string>();
        var resolvedBankName = string.Empty;
        var resolvedDisclaimer = string.Empty;

        foreach (var source in allSources)
        {
            try
            {
                var document = await source.LoadAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(document.BankName) && string.IsNullOrWhiteSpace(resolvedBankName))
                {
                    resolvedBankName = document.BankName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(document.Disclaimer) && string.IsNullOrWhiteSpace(resolvedDisclaimer))
                {
                    resolvedDisclaimer = document.Disclaimer.Trim();
                }

                foreach (var question in document.Questions)
                {
                    var normalizedQuestion = NormalizeQuestion(question);
                    if (normalizedQuestion is null)
                    {
                        continue;
                    }

                    mergedQuestions[normalizedQuestion.Id] = normalizedQuestion;
                }

                appliedSources.Add(source.Name);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Failed to refresh question bank from {SourceName}", source.Name);
                refreshWarnings.Add($"{source.Name}: {exception.Message}");
            }
        }

        if (mergedQuestions.Count == 0)
        {
            throw new InvalidOperationException(refreshWarnings.Count == 0
                ? "没有可用的题库源。"
                : $"所有题库源都加载失败：{string.Join("；", refreshWarnings)}");
        }

        var snapshot = mergedQuestions.Values
            .OrderBy(question => question.ReferenceInstitution, StringComparer.OrdinalIgnoreCase)
            .ThenBy(question => question.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var result = new QuestionBankRefreshResult
        {
            RefreshedAt = DateTimeOffset.UtcNow,
            QuestionCount = snapshot.Count,
            AppliedSources = appliedSources,
            RefreshWarnings = refreshWarnings
        };

        lock (syncRoot)
        {
            questions = snapshot;
            bankName = string.IsNullOrWhiteSpace(resolvedBankName) ? "海外艺术史中文练习题库" : resolvedBankName;
            disclaimer = string.IsNullOrWhiteSpace(resolvedDisclaimer) ? DefaultDisclaimer : resolvedDisclaimer;
            lastRefresh = result;
        }

        return Clone(result);
    }

    public List<ArtQuestion> GetQuestions()
    {
        lock (syncRoot)
        {
            return [.. questions.Select(Clone)];
        }
    }

    public QuestionBankRefreshResult? GetLastRefresh()
    {
        lock (syncRoot)
        {
            return lastRefresh is null ? null : Clone(lastRefresh);
        }
    }

    public string GetBankName()
    {
        lock (syncRoot)
        {
            return bankName;
        }
    }

    public string GetDisclaimer()
    {
        lock (syncRoot)
        {
            return disclaimer;
        }
    }

    private static ArtQuestion? NormalizeQuestion(ArtQuestion question)
    {
        if (string.IsNullOrWhiteSpace(question.Id) || string.IsNullOrWhiteSpace(question.ChinesePrompt) || string.IsNullOrWhiteSpace(question.CorrectAnswer))
        {
            return null;
        }

        return new ArtQuestion
        {
            Id = question.Id.Trim(),
            ReferenceInstitution = question.ReferenceInstitution.Trim(),
            ReferenceProgram = question.ReferenceProgram.Trim(),
            QuestionType = string.IsNullOrWhiteSpace(question.QuestionType) ? "单选题" : question.QuestionType.Trim(),
            ChinesePrompt = question.ChinesePrompt.Trim(),
            OriginalPrompt = question.OriginalPrompt.Trim(),
            Options = [.. question.Options.Where(static option => !string.IsNullOrWhiteSpace(option)).Select(static option => option.Trim())],
            CorrectAnswer = question.CorrectAnswer.Trim(),
            AnswerExplanation = question.AnswerExplanation.Trim(),
            Tags = [.. question.Tags.Where(static tag => !string.IsNullOrWhiteSpace(tag)).Select(static tag => tag.Trim())],
            SourceNote = question.SourceNote.Trim()
        };
    }

    private static ArtQuestion Clone(ArtQuestion question) => new()
    {
        Id = question.Id,
        ReferenceInstitution = question.ReferenceInstitution,
        ReferenceProgram = question.ReferenceProgram,
        QuestionType = question.QuestionType,
        ChinesePrompt = question.ChinesePrompt,
        OriginalPrompt = question.OriginalPrompt,
        Options = [.. question.Options],
        CorrectAnswer = question.CorrectAnswer,
        AnswerExplanation = question.AnswerExplanation,
        Tags = [.. question.Tags],
        SourceNote = question.SourceNote
    };

    private static QuestionBankRefreshResult Clone(QuestionBankRefreshResult result) => new()
    {
        RefreshedAt = result.RefreshedAt,
        QuestionCount = result.QuestionCount,
        AppliedSources = [.. result.AppliedSources],
        RefreshWarnings = [.. result.RefreshWarnings]
    };
}

public sealed class ExamPaperService(QuestionBankService questionBankService, IOptions<ArtExamOptions> optionsAccessor)
{
    private readonly object syncRoot = new();
    private ArtExamPaper? currentExam;

    public async Task<ArtExamPaper> GenerateAsync(int? requestedQuestionCount, string trigger, CancellationToken cancellationToken)
    {
        var availableQuestions = questionBankService.GetQuestions();
        if (availableQuestions.Count == 0)
        {
            await questionBankService.RefreshAsync(cancellationToken);
            availableQuestions = questionBankService.GetQuestions();
        }

        if (availableQuestions.Count == 0)
        {
            throw new InvalidOperationException("当前题库为空，无法生成试卷。");
        }

        var configuredQuestionCount = optionsAccessor.Value.DefaultQuestionCount;
        var questionCount = Math.Clamp(requestedQuestionCount ?? configuredQuestionCount, 1, availableQuestions.Count);
        var selectedQuestions = SelectRandomQuestions(availableQuestions, questionCount);
        var sourceSummary = string.Join(" / ", selectedQuestions.Select(question => question.ReferenceInstitution).Distinct(StringComparer.OrdinalIgnoreCase));
        var newExam = new ArtExamPaper
        {
            ExamId = $"exam-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            GeneratedAt = DateTimeOffset.UtcNow,
            Trigger = trigger,
            SourceSummary = sourceSummary,
            Questions = selectedQuestions
        };

        lock (syncRoot)
        {
            currentExam = newExam;
        }

        return Clone(newExam);
    }

    public ArtExamPaper? GetCurrentExam()
    {
        lock (syncRoot)
        {
            return currentExam is null ? null : Clone(currentExam);
        }
    }

    private static List<ArtQuestion> SelectRandomQuestions(List<ArtQuestion> availableQuestions, int questionCount)
    {
        var shuffledQuestions = availableQuestions.Select(Clone).ToList();
        for (var index = shuffledQuestions.Count - 1; index > 0; index--)
        {
            var swapIndex = Random.Shared.Next(index + 1);
            (shuffledQuestions[index], shuffledQuestions[swapIndex]) = (shuffledQuestions[swapIndex], shuffledQuestions[index]);
        }

        return [.. shuffledQuestions.Take(questionCount)];
    }

    private static ArtExamPaper Clone(ArtExamPaper paper) => new()
    {
        ExamId = paper.ExamId,
        GeneratedAt = paper.GeneratedAt,
        Trigger = paper.Trigger,
        SourceSummary = paper.SourceSummary,
        Questions = [.. paper.Questions.Select(Clone)]
    };

    private static ArtQuestion Clone(ArtQuestion question) => new()
    {
        Id = question.Id,
        ReferenceInstitution = question.ReferenceInstitution,
        ReferenceProgram = question.ReferenceProgram,
        QuestionType = question.QuestionType,
        ChinesePrompt = question.ChinesePrompt,
        OriginalPrompt = question.OriginalPrompt,
        Options = [.. question.Options],
        CorrectAnswer = question.CorrectAnswer,
        AnswerExplanation = question.AnswerExplanation,
        Tags = [.. question.Tags],
        SourceNote = question.SourceNote
    };
}
