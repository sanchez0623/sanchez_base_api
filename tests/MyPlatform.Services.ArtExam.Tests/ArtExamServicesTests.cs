using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyPlatform.Services.ArtExam;

namespace MyPlatform.Services.ArtExam.Tests;

public class ArtExamServicesTests
{
    [Fact]
    public async Task RefreshAsync_ShouldMergeDistinctQuestionIdsAcrossSources()
    {
        var questionBankService = new QuestionBankService(
        [
            new StubQuestionBankSource(
                "seed",
                new ArtQuestionBankDocument
                {
                    BankName = "MVP 题库",
                    Questions =
                    [
                        CreateQuestion("q-1", "问题 1"),
                        CreateQuestion("q-2", "问题 2")
                    ]
                }),
            new StubQuestionBankSource(
                "remote",
                new ArtQuestionBankDocument
                {
                    Questions =
                    [
                        CreateQuestion("q-2", "问题 2 - remote override"),
                        CreateQuestion("q-3", "问题 3")
                    ]
                })
        ],
        NullLogger<QuestionBankService>.Instance);

        var result = await questionBankService.RefreshAsync(CancellationToken.None);
        var questions = questionBankService.GetQuestions();

        Assert.Equal(3, result.QuestionCount);
        Assert.Equal(["seed", "remote"], result.AppliedSources);
        Assert.Equal(3, questions.Count);
        Assert.Contains(questions, question => question.Id == "q-2" && question.ChinesePrompt == "问题 2 - remote override");
    }

    [Fact]
    public async Task GenerateAsync_ShouldUseConfiguredDefaultCountAndPersistCurrentExam()
    {
        var questionBankService = new QuestionBankService(
        [
            new StubQuestionBankSource(
                "seed",
                new ArtQuestionBankDocument
                {
                    Questions =
                    [
                        CreateQuestion("q-1", "问题 1"),
                        CreateQuestion("q-2", "问题 2"),
                        CreateQuestion("q-3", "问题 3")
                    ]
                })
        ],
        NullLogger<QuestionBankService>.Instance);

        await questionBankService.RefreshAsync(CancellationToken.None);

        var examPaperService = new ExamPaperService(
            questionBankService,
            Options.Create(new ArtExamOptions { DefaultQuestionCount = 2 }));

        var exam = await examPaperService.GenerateAsync(null, "manual", CancellationToken.None);
        var currentExam = examPaperService.GetCurrentExam();

        Assert.Equal(2, exam.Questions.Count);
        Assert.NotNull(currentExam);
        Assert.Equal(exam.ExamId, currentExam!.ExamId);
        Assert.All(exam.Questions, question => Assert.False(string.IsNullOrWhiteSpace(question.CorrectAnswer)));
    }

    private static ArtQuestion CreateQuestion(string id, string prompt) => new()
    {
        Id = id,
        ChinesePrompt = prompt,
        CorrectAnswer = "标准答案",
        AnswerExplanation = "解析",
        ReferenceInstitution = "Test Institution",
        ReferenceProgram = "Test Program",
        Options = ["A", "B"]
    };

    private sealed class StubQuestionBankSource(string name, ArtQuestionBankDocument document) : IQuestionBankSource
    {
        public string Name => name;

        public Task<ArtQuestionBankDocument> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(document);
    }
}
