using FluentAssertions;
using MyPlatform.SDK.Saga.Models;
using MyPlatform.SDK.Saga.Persistence;
using Xunit;

namespace MyPlatform.SDK.Saga.Tests;

public class SagaStateStoreTests
{
    [Fact]
    public async Task SaveAsync_NewState_ShouldSaveSuccessfully()
    {
        // Arrange
        var store = new InMemorySagaStateStore();
        var state = new SagaState
        {
            SagaId = Guid.NewGuid().ToString(),
            SagaName = "TestSaga",
            Status = SagaStatus.Running
        };

        // Act
        await store.SaveAsync(state);
        var retrieved = await store.GetAsync(state.SagaId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.SagaId.Should().Be(state.SagaId);
        retrieved.SagaName.Should().Be(state.SagaName);
        retrieved.Status.Should().Be(SagaStatus.Running);
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsCorrectSagas()
    {
        // Arrange
        var store = new InMemorySagaStateStore();
        var runningState = new SagaState { SagaId = "1", SagaName = "Saga1", Status = SagaStatus.Running };
        var completedState = new SagaState { SagaId = "2", SagaName = "Saga2", Status = SagaStatus.Completed };

        await store.SaveAsync(runningState);
        await store.SaveAsync(completedState);

        // Act
        var runningSagas = await store.GetByStatusAsync(SagaStatus.Running);
        var completedSagas = await store.GetByStatusAsync(SagaStatus.Completed);

        // Assert
        runningSagas.Should().HaveCount(1);
        runningSagas.First().SagaId.Should().Be("1");

        completedSagas.Should().HaveCount(1);
        completedSagas.First().SagaId.Should().Be("2");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveState()
    {
        // Arrange
        var store = new InMemorySagaStateStore();
        var state = new SagaState { SagaId = "test-saga", SagaName = "TestSaga" };
        await store.SaveAsync(state);

        // Act
        await store.DeleteAsync(state.SagaId);
        var retrieved = await store.GetAsync(state.SagaId);

        // Assert
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GetPendingRetriesAsync_ReturnsOnlyRetriableStates()
    {
        // Arrange
        var store = new InMemorySagaStateStore();
        var pendingRetry = new SagaState
        {
            SagaId = "1",
            SagaName = "Saga1",
            Status = SagaStatus.Suspended,
            NextRetryAt = DateTime.UtcNow.AddMinutes(-5) // Past due
        };
        var notYetDue = new SagaState
        {
            SagaId = "2",
            SagaName = "Saga2",
            Status = SagaStatus.Suspended,
            NextRetryAt = DateTime.UtcNow.AddMinutes(5) // Future
        };
        var noRetry = new SagaState
        {
            SagaId = "3",
            SagaName = "Saga3",
            Status = SagaStatus.Completed
        };

        await store.SaveAsync(pendingRetry);
        await store.SaveAsync(notYetDue);
        await store.SaveAsync(noRetry);

        // Act
        var retryable = await store.GetPendingRetriesAsync();

        // Assert
        retryable.Should().HaveCount(1);
        retryable.First().SagaId.Should().Be("1");
    }
}
