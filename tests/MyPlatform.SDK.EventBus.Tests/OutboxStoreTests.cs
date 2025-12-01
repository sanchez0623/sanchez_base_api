using FluentAssertions;
using MyPlatform.SDK.EventBus.Outbox;
using Xunit;

namespace MyPlatform.SDK.EventBus.Tests;

public class OutboxStoreTests
{
    [Fact]
    public async Task SaveAsync_NewMessage_ShouldSaveSuccessfully()
    {
        // Arrange
        var store = new InMemoryOutboxStore();
        var message = new OutboxMessage
        {
            EventType = "TestEvent",
            Payload = "{\"data\":\"test\"}"
        };

        // Act
        await store.SaveAsync(message);
        var unprocessed = await store.GetUnprocessedAsync();

        // Assert
        unprocessed.Should().HaveCount(1);
        unprocessed.First().Id.Should().Be(message.Id);
        unprocessed.First().EventType.Should().Be("TestEvent");
    }

    [Fact]
    public async Task GetUnprocessedAsync_ShouldReturnOnlyUnprocessedMessages()
    {
        // Arrange
        var store = new InMemoryOutboxStore();
        var unprocessedMessage = new OutboxMessage { EventType = "Event1", Payload = "{}" };
        var processedMessage = new OutboxMessage { EventType = "Event2", Payload = "{}", IsProcessed = true };

        await store.SaveAsync(unprocessedMessage);
        await store.SaveAsync(processedMessage);

        // Act
        var unprocessed = await store.GetUnprocessedAsync();

        // Assert
        unprocessed.Should().HaveCount(1);
        unprocessed.First().EventType.Should().Be("Event1");
    }

    [Fact]
    public async Task MarkAsProcessedAsync_ShouldMarkMessageAsProcessed()
    {
        // Arrange
        var store = new InMemoryOutboxStore();
        var message = new OutboxMessage { EventType = "TestEvent", Payload = "{}" };
        await store.SaveAsync(message);

        // Act
        await store.MarkAsProcessedAsync(message.Id);
        var unprocessed = await store.GetUnprocessedAsync();

        // Assert
        unprocessed.Should().BeEmpty();
    }

    [Fact]
    public async Task MarkAsFailedAsync_ShouldIncrementRetryCount()
    {
        // Arrange
        var store = new InMemoryOutboxStore();
        var message = new OutboxMessage { EventType = "TestEvent", Payload = "{}" };
        await store.SaveAsync(message);

        // Act
        await store.MarkAsFailedAsync(message.Id, "Test error");
        var unprocessed = await store.GetUnprocessedAsync();

        // Assert
        unprocessed.Should().HaveCount(1);
        unprocessed.First().RetryCount.Should().Be(1);
        unprocessed.First().Error.Should().Be("Test error");
    }

    [Fact]
    public async Task DeleteProcessedAsync_ShouldRemoveOldProcessedMessages()
    {
        // Arrange
        var store = new InMemoryOutboxStore();
        var oldMessage = new OutboxMessage
        {
            EventType = "OldEvent",
            Payload = "{}",
            IsProcessed = true
        };
        await store.SaveAsync(oldMessage);
        await store.MarkAsProcessedAsync(oldMessage.Id);

        var newMessage = new OutboxMessage
        {
            EventType = "NewEvent",
            Payload = "{}",
            IsProcessed = true
        };
        await store.SaveAsync(newMessage);
        await store.MarkAsProcessedAsync(newMessage.Id);

        // Act - Delete messages processed before 1 hour from now (should delete both)
        var deleted = await store.DeleteProcessedAsync(DateTime.UtcNow.AddHours(1));

        // Assert
        deleted.Should().Be(2);
    }

    [Fact]
    public async Task GetUnprocessedAsync_ShouldRespectBatchSize()
    {
        // Arrange
        var store = new InMemoryOutboxStore();
        for (var i = 0; i < 10; i++)
        {
            await store.SaveAsync(new OutboxMessage { EventType = $"Event{i}", Payload = "{}" });
        }

        // Act
        var batch = await store.GetUnprocessedAsync(5);

        // Assert
        batch.Should().HaveCount(5);
    }
}
