using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.Saga.Extensions;
using MyPlatform.SDK.Saga.Persistence;
using Xunit;

namespace MyPlatform.SDK.Saga.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPlatformSaga_WithInMemoryConfig_RegistersInMemoryStore()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Saga:StateStore", "InMemory" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddPlatformSaga(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var store = provider.GetRequiredService<ISagaStateStore>();
        store.Should().BeOfType<InMemorySagaStateStore>();
    }

    [Fact]
    public void AddPlatformSaga_WithDatabaseConfig_RegistersEfCoreStore()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Saga:StateStore", "Database" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddPlatformSaga(configuration);

        // Assert - verify registration type, not resolved instance (as DbContext is not available)
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISagaStateStore));
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(EfCoreSagaStateStore));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddPlatformSaga_WithRedisConfig_RegistersRedisStore()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Saga:StateStore", "Redis" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddPlatformSaga(configuration);

        // Assert - verify registration type, not resolved instance (as Redis connection is not available)
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISagaStateStore));
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(RedisSagaStateStore));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPlatformSaga_WithOptionsAction_RegistersInMemoryStore()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPlatformSaga(options =>
        {
            options.DefaultTimeoutSeconds = 600;
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var store = provider.GetRequiredService<ISagaStateStore>();
        store.Should().BeOfType<InMemorySagaStateStore>();
    }

    [Fact]
    public void AddSagaStateStore_ReplacesExistingRegistration()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Saga:StateStore", "InMemory" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddPlatformSaga(configuration);

        // Act - Replace with a different store type
        services.AddSagaStateStore<InMemorySagaStateStore>();

        // Assert - Should have only one registration
        var registrations = services.Where(d => d.ServiceType == typeof(ISagaStateStore)).ToList();
        registrations.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("inmemory")]
    [InlineData("InMemory")]
    [InlineData("INMEMORY")]
    public void AddPlatformSaga_StateStoreConfig_IsCaseInsensitive(string stateStoreValue)
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Saga:StateStore", stateStoreValue }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddPlatformSaga(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var store = provider.GetRequiredService<ISagaStateStore>();
        store.Should().BeOfType<InMemorySagaStateStore>();
    }

    [Theory]
    [InlineData("database")]
    [InlineData("Database")]
    [InlineData("efcore")]
    [InlineData("EfCore")]
    public void AddPlatformSaga_DatabaseConfig_IsCaseInsensitive(string stateStoreValue)
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Saga:StateStore", stateStoreValue }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddPlatformSaga(configuration);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISagaStateStore));
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(EfCoreSagaStateStore));
    }
}
