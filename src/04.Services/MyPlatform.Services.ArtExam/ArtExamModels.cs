namespace MyPlatform.Services.ArtExam;

public sealed class ArtExamOptions
{
    public const string SectionName = "ArtExam";

    public string QuestionBankSeedFile { get; init; } = "Data/art-question-bank.seed.json";
    public List<string> RemoteQuestionBankSources { get; init; } = [];
    public int DefaultQuestionCount { get; init; } = 5;
    public int QuestionBankRefreshIntervalMinutes { get; init; } = 120;
    public int AutoGenerateIntervalMinutes { get; init; } = 30;
}

public sealed class ArtQuestionBankDocument
{
    public string BankName { get; init; } = string.Empty;
    public string Disclaimer { get; init; } = string.Empty;
    public List<ArtQuestion> Questions { get; init; } = [];
}

public sealed class ArtQuestion
{
    public string Id { get; init; } = string.Empty;
    public string ReferenceInstitution { get; init; } = string.Empty;
    public string ReferenceProgram { get; init; } = string.Empty;
    public string QuestionType { get; init; } = "单选题";
    public string ChinesePrompt { get; init; } = string.Empty;
    public string OriginalPrompt { get; init; } = string.Empty;
    public List<string> Options { get; init; } = [];
    public string CorrectAnswer { get; init; } = string.Empty;
    public string AnswerExplanation { get; init; } = string.Empty;
    public List<string> Tags { get; init; } = [];
    public string SourceNote { get; init; } = string.Empty;
}

public sealed class ArtExamPaper
{
    public string ExamId { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; init; }
    public string Trigger { get; init; } = string.Empty;
    public string SourceSummary { get; init; } = string.Empty;
    public List<ArtQuestion> Questions { get; init; } = [];
}

public sealed class QuestionBankRefreshResult
{
    public DateTimeOffset RefreshedAt { get; init; }
    public int QuestionCount { get; init; }
    public List<string> AppliedSources { get; init; } = [];
    public List<string> RefreshWarnings { get; init; } = [];
}

public sealed class ArtExamDashboard
{
    public string Title { get; init; } = string.Empty;
    public string BankName { get; init; } = string.Empty;
    public string Disclaimer { get; init; } = string.Empty;
    public int TotalQuestions { get; init; }
    public DateTimeOffset? LastQuestionBankRefreshAt { get; init; }
    public List<string> AppliedSources { get; init; } = [];
    public List<string> RefreshWarnings { get; init; } = [];
    public ArtExamPaper? CurrentExam { get; init; }
    public int DefaultQuestionCount { get; init; }
    public int QuestionBankRefreshIntervalMinutes { get; init; }
    public int AutoGenerateIntervalMinutes { get; init; }
    public List<string> RemoteQuestionBankSources { get; init; } = [];
}

public sealed class GenerateExamRequest
{
    public int? QuestionCount { get; init; }
}
