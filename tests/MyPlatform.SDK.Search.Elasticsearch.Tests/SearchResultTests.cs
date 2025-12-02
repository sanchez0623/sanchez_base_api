using FluentAssertions;
using MyPlatform.SDK.Search.Elasticsearch.Abstractions;
using Xunit;

namespace MyPlatform.SDK.Search.Elasticsearch.Tests;

public class SearchResultTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var result = new SearchResult<TestSearchDocument>();

        // Assert
        result.Documents.Should().BeEmpty();
        result.Total.Should().Be(0);
        result.Took.Should().Be(0);
        result.Highlights.Should().BeEmpty();
    }

    [Fact]
    public void SetProperties_ShouldWorkCorrectly()
    {
        // Arrange
        var docs = new List<TestSearchDocument>
        {
            new() { DocumentId = "1", Title = "Test" }
        };

        var highlights = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>>
        {
            ["1"] = new Dictionary<string, IReadOnlyList<string>>
            {
                ["title"] = new List<string> { "<em>Test</em>" }
            }
        };

        // Act
        var result = new SearchResult<TestSearchDocument>
        {
            Documents = docs,
            Total = 100,
            Took = 50,
            Highlights = highlights
        };

        // Assert
        result.Documents.Should().HaveCount(1);
        result.Documents[0].DocumentId.Should().Be("1");
        result.Total.Should().Be(100);
        result.Took.Should().Be(50);
        result.Highlights.Should().HaveCount(1);
    }
}
