using FluentAssertions;
using Moq;
using MyPlatform.Infrastructure.Redis.Services;
using MyPlatform.SDK.Idempotency.Services;
using Xunit;

namespace MyPlatform.SDK.Idempotency.Tests;

public class IdempotencyServiceTests
{
    private readonly Mock<IRedisCacheService> _cacheServiceMock;
    private readonly Mock<IDistributedLockService> _lockServiceMock;

    public IdempotencyServiceTests()
    {
        _cacheServiceMock = new Mock<IRedisCacheService>();
        _lockServiceMock = new Mock<IDistributedLockService>();
    }

    [Fact]
    public async Task TryAcquireAsync_NoCachedResult_ShouldAcquireLock()
    {
        // Arrange
        var lockMock = new Mock<IDistributedLock>();
        lockMock.Setup(l => l.IsAcquired).Returns(true);

        _cacheServiceMock
            .Setup(c => c.GetAsync<IdempotentResult>(It.IsAny<string>()))
            .ReturnsAsync((IdempotentResult?)null);

        _lockServiceMock
            .Setup(l => l.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(lockMock.Object);

        var service = new RedisIdempotencyService(
            _cacheServiceMock.Object,
            _lockServiceMock.Object,
            TimeSpan.FromHours(24),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(5));

        // Act
        var (shouldProceed, @lock, cachedResult) = await service.TryAcquireAsync("test-key");

        // Assert
        shouldProceed.Should().BeTrue();
        @lock.Should().NotBeNull();
        cachedResult.Should().BeNull();
    }

    [Fact]
    public async Task TryAcquireAsync_WithCachedResult_ShouldReturnCachedResult()
    {
        // Arrange
        var expectedResult = new IdempotentResult
        {
            StatusCode = 200,
            Body = "cached response"
        };

        _cacheServiceMock
            .Setup(c => c.GetAsync<IdempotentResult>(It.IsAny<string>()))
            .ReturnsAsync(expectedResult);

        var service = new RedisIdempotencyService(
            _cacheServiceMock.Object,
            _lockServiceMock.Object,
            TimeSpan.FromHours(24),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(5));

        // Act
        var (shouldProceed, @lock, cachedResult) = await service.TryAcquireAsync("test-key");

        // Assert
        shouldProceed.Should().BeFalse();
        @lock.Should().BeNull();
        cachedResult.Should().NotBeNull();
        cachedResult!.StatusCode.Should().Be(200);
        cachedResult.Body.Should().Be("cached response");
    }

    [Fact]
    public async Task StoreResultAsync_ShouldCacheResult()
    {
        // Arrange
        var result = new IdempotentResult
        {
            StatusCode = 201,
            Body = "created"
        };

        _cacheServiceMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IdempotentResult>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        var service = new RedisIdempotencyService(
            _cacheServiceMock.Object,
            _lockServiceMock.Object,
            TimeSpan.FromHours(24),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(5));

        // Act
        await service.StoreResultAsync("test-key", result);

        // Assert
        _cacheServiceMock.Verify(c => c.SetAsync(
            It.Is<string>(s => s.Contains("test-key")),
            It.Is<IdempotentResult>(r => r.StatusCode == 201),
            It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
    {
        // Arrange
        _cacheServiceMock
            .Setup(c => c.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = new RedisIdempotencyService(
            _cacheServiceMock.Object,
            _lockServiceMock.Object,
            TimeSpan.FromHours(24),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(5));

        // Act
        var exists = await service.ExistsAsync("test-key");

        // Assert
        exists.Should().BeTrue();
    }
}
