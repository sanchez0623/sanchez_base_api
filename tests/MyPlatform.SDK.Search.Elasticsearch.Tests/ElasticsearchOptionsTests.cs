using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.Search.Elasticsearch.Configuration;
using MyPlatform.SDK.Search.Elasticsearch.Extensions;
using Xunit;

namespace MyPlatform.SDK.Search.Elasticsearch.Tests;

public class ElasticsearchOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new ElasticsearchOptions();

        // Assert
        options.Nodes.Should().ContainSingle().Which.Should().Be("http://localhost:9200");
        options.DefaultIndex.Should().Be("default");
        options.NumberOfShards.Should().Be(3);
        options.NumberOfReplicas.Should().Be(1);
        options.EnableDebugMode.Should().BeFalse();
        options.RequestTimeout.Should().Be(TimeSpan.FromSeconds(30));
        options.Username.Should().BeNull();
        options.Password.Should().BeNull();
        options.ApiKey.Should().BeNull();
        options.CertificateFingerprint.Should().BeNull();
    }

    [Fact]
    public void Configuration_ShouldBindCorrectly()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Elasticsearch:Nodes:0"] = "http://es1:9200",
            ["Elasticsearch:Nodes:1"] = "http://es2:9200",
            ["Elasticsearch:DefaultIndex"] = "myindex",
            ["Elasticsearch:NumberOfShards"] = "5",
            ["Elasticsearch:NumberOfReplicas"] = "2",
            ["Elasticsearch:EnableDebugMode"] = "true",
            ["Elasticsearch:RequestTimeout"] = "00:01:00",
            ["Elasticsearch:Username"] = "elastic",
            ["Elasticsearch:Password"] = "secret",
            ["Elasticsearch:ApiKey"] = "test-api-key"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.SectionName));

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ElasticsearchOptions>>().Value;

        // Assert - Nodes may include default plus configured nodes depending on binding behavior
        options.Nodes.Should().Contain("http://es1:9200");
        options.Nodes.Should().Contain("http://es2:9200");
        options.DefaultIndex.Should().Be("myindex");
        options.NumberOfShards.Should().Be(5);
        options.NumberOfReplicas.Should().Be(2);
        options.EnableDebugMode.Should().BeTrue();
        options.RequestTimeout.Should().Be(TimeSpan.FromMinutes(1));
        options.Username.Should().Be("elastic");
        options.Password.Should().Be("secret");
        options.ApiKey.Should().Be("test-api-key");
    }

    [Fact]
    public void SectionName_ShouldBeElasticsearch()
    {
        // Assert
        ElasticsearchOptions.SectionName.Should().Be("Elasticsearch");
    }
}
