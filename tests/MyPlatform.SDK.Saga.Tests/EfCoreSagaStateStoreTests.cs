using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyPlatform.SDK.Saga.Models;
using MyPlatform.SDK.Saga.Persistence;
using Xunit;

namespace MyPlatform.SDK.Saga.Tests;

public class EfCoreSagaStateStoreTests
{
    private static SagaDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<SagaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new SagaDbContext(options);
    }

    [Fact]
    public async Task SaveAsync_NewState_ShouldSaveSuccessfully()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var store = new EfCoreSagaStateStore(context);
        var state = new SagaState
        {
            SagaId = Guid.NewGuid().ToString(),
            SagaName = "TestSaga",
            Status = SagaStatus.Running,
            Data = "{\"test\":\"data\"}"
        };

        // Act
        await store.SaveAsync(state);
        var retrieved = await store.GetAsync(state.SagaId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.SagaId.Should().Be(state.SagaId);
        retrieved.SagaName.Should().Be(state.SagaName);
        retrieved.Status.Should().Be(SagaStatus.Running);
        retrieved.Data.Should().Be("{\"test\":\"data\"}");
    }

    [Fact]
    public async Task SaveAsync_ExistingState_ShouldUpdateSuccessfully()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var store = new EfCoreSagaStateStore(context);
        var state = new SagaState
        {
            SagaId = "test-saga-1",
            SagaName = "TestSaga",
            Status = SagaStatus.Running
        };

        await store.SaveAsync(state);

        // Act
        state.Status = SagaStatus.Completed;
        state.CompletedAt = DateTime.UtcNow;
        await store.SaveAsync(state);
        var retrieved = await store.GetAsync(state.SagaId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(SagaStatus.Completed);
        retrieved.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsCorrectSagas()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var store = new EfCoreSagaStateStore(context);
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
        using var context = CreateInMemoryContext();
        var store = new EfCoreSagaStateStore(context);
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
        using var context = CreateInMemoryContext();
        var store = new EfCoreSagaStateStore(context);
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

    [Fact]
    public async Task SaveAsync_WithSteps_ShouldPersistSteps()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var store = new EfCoreSagaStateStore(context);
        var state = new SagaState
        {
            SagaId = Guid.NewGuid().ToString(),
            SagaName = "TestSaga",
            Status = SagaStatus.Running,
            Steps =
            [
                new SagaStepState { StepIndex = 0, StepName = "Step1", Status = SagaStepStatus.Completed },
                new SagaStepState { StepIndex = 1, StepName = "Step2", Status = SagaStepStatus.Executing }
            ]
        };

        // Act
        await store.SaveAsync(state);
        var retrieved = await store.GetAsync(state.SagaId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Steps.Should().HaveCount(2);
        retrieved.Steps[0].StepName.Should().Be("Step1");
        retrieved.Steps[1].StepName.Should().Be("Step2");
    }

    [Fact]
    public async Task SaveAsync_WithTenantAndCorrelation_ShouldPersist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var store = new EfCoreSagaStateStore(context);
        var state = new SagaState
        {
            SagaId = Guid.NewGuid().ToString(),
            SagaName = "TestSaga",
            Status = SagaStatus.Running,
            TenantId = "tenant-123",
            CorrelationId = "corr-456"
        };

        // Act
        await store.SaveAsync(state);
        var retrieved = await store.GetAsync(state.SagaId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.TenantId.Should().Be("tenant-123");
        retrieved.CorrelationId.Should().Be("corr-456");
    }
}
