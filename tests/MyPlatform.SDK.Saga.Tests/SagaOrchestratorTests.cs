using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MyPlatform.SDK.Saga.Abstractions;
using MyPlatform.SDK.Saga.Configuration;
using MyPlatform.SDK.Saga.Models;
using MyPlatform.SDK.Saga.Orchestration;
using MyPlatform.SDK.Saga.Persistence;
using Xunit;

namespace MyPlatform.SDK.Saga.Tests;

public class SagaOrchestratorTests
{
    private readonly Mock<ISagaStateStore> _stateStoreMock;
    private readonly Mock<ILogger<TestOrderSagaOrchestrator>> _loggerMock;
    private readonly IOptions<SagaOptions> _options;

    public SagaOrchestratorTests()
    {
        _stateStoreMock = new Mock<ISagaStateStore>();
        _loggerMock = new Mock<ILogger<TestOrderSagaOrchestrator>>();
        _options = Options.Create(new SagaOptions
        {
            DefaultRetryCount = 3,
            DefaultTimeoutSeconds = 300
        });
    }

    [Fact]
    public async Task ExecuteAsync_AllStepsSucceed_ReturnSuccess()
    {
        // Arrange
        var stateStore = new InMemorySagaStateStore();
        var orchestrator = new TestOrderSagaOrchestrator(stateStore, _options, _loggerMock.Object);
        var data = new TestOrderData { OrderId = "ORD-001", Amount = 100 };

        // Act
        var result = await orchestrator.ExecuteAsync(data);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be(SagaStatus.Completed);
        result.Data.Should().NotBeNull();
        result.Data!.IsReservationCompleted.Should().BeTrue();
        result.Data.IsPaymentCompleted.Should().BeTrue();
        result.Data.IsShippingCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithCorrelationId_StoresCorrelationId()
    {
        // Arrange
        var stateStore = new InMemorySagaStateStore();
        var orchestrator = new TestOrderSagaOrchestrator(stateStore, _options, _loggerMock.Object);
        var data = new TestOrderData { OrderId = "ORD-002", Amount = 200 };
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = await orchestrator.ExecuteAsync(data, correlationId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var state = await stateStore.GetAsync(result.SagaId);
        state.Should().NotBeNull();
        state!.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public async Task ExecuteAsync_WithTenantId_StoresTenantId()
    {
        // Arrange
        var stateStore = new InMemorySagaStateStore();
        var orchestrator = new TestOrderSagaOrchestrator(stateStore, _options, _loggerMock.Object);
        var data = new TestOrderData { OrderId = "ORD-003", Amount = 300 };
        var tenantId = "tenant-123";

        // Act
        var result = await orchestrator.ExecuteAsync(data, tenantId: tenantId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var state = await stateStore.GetAsync(result.SagaId);
        state.Should().NotBeNull();
        state!.TenantId.Should().Be(tenantId);
    }
}

// Test data class
public class TestOrderData
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsReservationCompleted { get; set; }
    public bool IsPaymentCompleted { get; set; }
    public bool IsShippingCompleted { get; set; }
}

// Test saga orchestrator
public class TestOrderSagaOrchestrator : SagaOrchestrator<TestOrderData>
{
    public TestOrderSagaOrchestrator(
        ISagaStateStore stateStore,
        IOptions<SagaOptions> options,
        ILogger<TestOrderSagaOrchestrator> logger)
        : base(stateStore, options, logger)
    {
    }

    protected override string SagaName => "TestOrderSaga";

    protected override void ConfigureSteps()
    {
        AddStep(new ReserveInventoryStep());
        AddStep(new ProcessPaymentStep());
        AddStep(new CreateShippingStep());
    }
}

// Test steps
public class ReserveInventoryStep : SagaStepBase<TestOrderData>
{
    public override string StepName => "ReserveInventory";

    public override Task ExecuteAsync(SagaContext<TestOrderData> context, CancellationToken cancellationToken = default)
    {
        context.Data.IsReservationCompleted = true;
        return Task.CompletedTask;
    }

    public override Task CompensateAsync(SagaContext<TestOrderData> context, CancellationToken cancellationToken = default)
    {
        context.Data.IsReservationCompleted = false;
        return Task.CompletedTask;
    }
}

public class ProcessPaymentStep : SagaStepBase<TestOrderData>
{
    public override string StepName => "ProcessPayment";

    public override Task ExecuteAsync(SagaContext<TestOrderData> context, CancellationToken cancellationToken = default)
    {
        context.Data.IsPaymentCompleted = true;
        return Task.CompletedTask;
    }

    public override Task CompensateAsync(SagaContext<TestOrderData> context, CancellationToken cancellationToken = default)
    {
        context.Data.IsPaymentCompleted = false;
        return Task.CompletedTask;
    }
}

public class CreateShippingStep : SagaStepBase<TestOrderData>
{
    public override string StepName => "CreateShipping";

    public override Task ExecuteAsync(SagaContext<TestOrderData> context, CancellationToken cancellationToken = default)
    {
        context.Data.IsShippingCompleted = true;
        return Task.CompletedTask;
    }

    public override Task CompensateAsync(SagaContext<TestOrderData> context, CancellationToken cancellationToken = default)
    {
        context.Data.IsShippingCompleted = false;
        return Task.CompletedTask;
    }
}
