using FluentAssertions;
using MyPlatform.SDK.Search.Elasticsearch.Abstractions;
using MyPlatform.SDK.Search.Elasticsearch.Builders;
using Xunit;

namespace MyPlatform.SDK.Search.Elasticsearch.Tests;

public class SearchQueryBuilderTests
{
    [Fact]
    public void Create_ShouldReturnNewBuilder()
    {
        // Act
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithNoConditions_ShouldReturnEmptyQuery()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.Build();

        // Assert
        query.Should().NotBeNull();
        query.Conditions.Should().BeEmpty();
        query.PageIndex.Should().Be(1);
        query.PageSize.Should().Be(20);
    }

    [Fact]
    public void WithTerm_ShouldAddTermCondition()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.WithTerm(d => d.Status, "active").Build();

        // Assert
        query.Conditions.Should().HaveCount(1);
        query.Conditions[0].Field.Should().Be("status");
        query.Conditions[0].Type.Should().Be(QueryType.Term);
        query.Conditions[0].Value.Should().Be("active");
    }

    [Fact]
    public void WithMatch_ShouldAddMatchCondition()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.WithMatch(d => d.Title, "test document").Build();

        // Assert
        query.Conditions.Should().HaveCount(1);
        query.Conditions[0].Field.Should().Be("title");
        query.Conditions[0].Type.Should().Be(QueryType.Match);
        query.Conditions[0].Value.Should().Be("test document");
    }

    [Fact]
    public void WithRange_ShouldAddRangeCondition()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.WithRange(d => d.Price, 100m, 500m).Build();

        // Assert
        query.Conditions.Should().HaveCount(1);
        query.Conditions[0].Field.Should().Be("price");
        query.Conditions[0].Type.Should().Be(QueryType.Range);
        query.Conditions[0].MinValue.Should().Be(100m);
        query.Conditions[0].MaxValue.Should().Be(500m);
    }

    [Fact]
    public void WithDateRange_ShouldAddRangeConditionForDates()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();
        var from = new DateTime(2024, 1, 1);
        var to = new DateTime(2024, 12, 31);

        // Act
        var query = builder.WithDateRange(d => d.CreatedAt, from, to).Build();

        // Assert
        query.Conditions.Should().HaveCount(1);
        query.Conditions[0].Field.Should().Be("createdAt");
        query.Conditions[0].Type.Should().Be(QueryType.Range);
        query.Conditions[0].MinValue.Should().Be(from);
        query.Conditions[0].MaxValue.Should().Be(to);
    }

    [Fact]
    public void WithTerms_ShouldAddTermsCondition()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();
        var values = new[] { "cat1", "cat2", "cat3" };

        // Act
        var query = builder.WithTerms(d => d.Status, values).Build();

        // Assert
        query.Conditions.Should().HaveCount(1);
        query.Conditions[0].Field.Should().Be("status");
        query.Conditions[0].Type.Should().Be(QueryType.Terms);
    }

    [Fact]
    public void WithFullText_ShouldAddMultiMatchCondition()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.WithFullText("search text", d => d.Title, d => d.Description).Build();

        // Assert
        query.Conditions.Should().HaveCount(1);
        query.Conditions[0].Type.Should().Be(QueryType.MultiMatch);
        query.Conditions[0].Field.Should().Be("title");
        query.Conditions[0].AdditionalFields.Should().Contain("description");
        query.Conditions[0].Value.Should().Be("search text");
    }

    [Fact]
    public void WithNested_ShouldAddNestedCondition()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.WithNested("items", n => n.WithTerm("items.brandId", 10)).Build();

        // Assert
        query.Conditions.Should().HaveCount(1);
        query.Conditions[0].Type.Should().Be(QueryType.Nested);
        query.Conditions[0].NestedPath.Should().Be("items");
        query.Conditions[0].NestedConditions.Should().HaveCount(1);
    }

    [Fact]
    public void OrderBy_ShouldAddAscendingSortSpecification()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.OrderBy(d => d.CreatedAt).Build();

        // Assert
        query.SortSpecifications.Should().HaveCount(1);
        query.SortSpecifications[0].Field.Should().Be("createdAt");
        query.SortSpecifications[0].Descending.Should().BeFalse();
    }

    [Fact]
    public void OrderByDescending_ShouldAddDescendingSortSpecification()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.OrderByDescending(d => d.CreatedAt).Build();

        // Assert
        query.SortSpecifications.Should().HaveCount(1);
        query.SortSpecifications[0].Field.Should().Be("createdAt");
        query.SortSpecifications[0].Descending.Should().BeTrue();
    }

    [Fact]
    public void WithPaging_ShouldSetPagingParameters()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.WithPaging(pageIndex: 3, pageSize: 50).Build();

        // Assert
        query.PageIndex.Should().Be(3);
        query.PageSize.Should().Be(50);
    }

    [Fact]
    public void WithPaging_ShouldClampInvalidValues()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.WithPaging(pageIndex: 0, pageSize: 0).Build();

        // Assert
        query.PageIndex.Should().Be(1);
        query.PageSize.Should().Be(1);
    }

    [Fact]
    public void WithHighlight_ShouldAddHighlightFields()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.WithHighlight(d => d.Title, d => d.Description).Build();

        // Assert
        query.HighlightFields.Should().HaveCount(2);
        query.HighlightFields.Should().Contain("title");
        query.HighlightFields.Should().Contain("description");
    }

    [Fact]
    public void WithPrefix_ShouldAddPrefixCondition()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.WithPrefix(d => d.Title, "test").Build();

        // Assert
        query.Conditions.Should().HaveCount(1);
        query.Conditions[0].Field.Should().Be("title");
        query.Conditions[0].Type.Should().Be(QueryType.Prefix);
        query.Conditions[0].Value.Should().Be("test");
    }

    [Fact]
    public void WithWildcard_ShouldAddWildcardCondition()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.WithWildcard(d => d.Title, "test*").Build();

        // Assert
        query.Conditions.Should().HaveCount(1);
        query.Conditions[0].Field.Should().Be("title");
        query.Conditions[0].Type.Should().Be(QueryType.Wildcard);
        query.Conditions[0].Value.Should().Be("test*");
    }

    [Fact]
    public void WithExists_ShouldAddExistsCondition()
    {
        // Arrange
        var builder = SearchQueryBuilder<TestSearchDocument>.Create();

        // Act
        var query = builder.WithExists(d => d.Description).Build();

        // Assert
        query.Conditions.Should().HaveCount(1);
        query.Conditions[0].Field.Should().Be("description");
        query.Conditions[0].Type.Should().Be(QueryType.Exists);
    }

    [Fact]
    public void ChainedMethods_ShouldAccumulateConditions()
    {
        // Arrange & Act
        var query = SearchQueryBuilder<TestSearchDocument>.Create()
            .WithTerm(d => d.Status, "active")
            .WithMatch(d => d.Title, "test")
            .WithRange(d => d.Price, 100m, 500m)
            .OrderByDescending(d => d.CreatedAt)
            .WithPaging(2, 25)
            .Build();

        // Assert
        query.Conditions.Should().HaveCount(3);
        query.SortSpecifications.Should().HaveCount(1);
        query.PageIndex.Should().Be(2);
        query.PageSize.Should().Be(25);
    }
}
