# Ship24X7 Integration Tests

This project contains integration tests for inter-service event flows in the Ship24X7 platform.

## Overview

These tests validate the asynchronous event-driven communication between microservices using RabbitMQ. The tests use Testcontainers to spin up a real RabbitMQ instance for testing, ensuring accurate integration testing without mocking the message broker.

## Test Coverage

### Event Flow Tests

1. **ShipmentBookedEventFlowTests**
   - Tests ShipmentBooked event publishing from Shipment Service
   - Validates event delivery to Notification Service
   - Verifies CorrelationId propagation
   - Tests multiple event delivery

2. **PaymentCapturedEventFlowTests**
   - Tests PaymentCaptured event publishing from Payment Service
   - Validates event delivery to both Shipment Service and Notification Service
   - Verifies Razorpay payment details in events
   - Tests multi-consumer event delivery

3. **ShipmentDeliveredEventFlowTests**
   - Tests ShipmentDelivered event publishing from Tracking Service
   - Validates event delivery to Notification Service
   - Verifies GPS coordinates and delivery proof data
   - Tests delivery timestamp accuracy

4. **DeadLetterQueueTests**
   - Tests DLQ routing after 3 failed retry attempts
   - Validates exponential backoff retry logic
   - Verifies CorrelationId preservation in DLQ
   - Tests successful processing after retry

5. **CorrelationIdPropagationTests**
   - Tests CorrelationId propagation across all services
   - Validates unique CorrelationId per request
   - Tests end-to-end tracing through event chains

## Prerequisites

- .NET 10.0 SDK
- Docker Desktop (for Testcontainers)
- At least 2GB of available RAM for Docker containers

## Running the Tests

### Run All Tests

```bash
dotnet test tests/Ship24X7.IntegrationTests/Ship24X7.IntegrationTests.csproj
```

### Run Specific Test Class

```bash
dotnet test tests/Ship24X7.IntegrationTests/Ship24X7.IntegrationTests.csproj --filter "FullyQualifiedName~ShipmentBookedEventFlowTests"
```

### Run with Verbose Output

```bash
dotnet test tests/Ship24X7.IntegrationTests/Ship24X7.IntegrationTests.csproj --logger "console;verbosity=detailed"
```

### Run with Code Coverage

```bash
dotnet test tests/Ship24X7.IntegrationTests/Ship24X7.IntegrationTests.csproj --collect:"XPlat Code Coverage"
```

## Test Architecture

### RabbitMqTestFixture

The `RabbitMqTestFixture` class manages the lifecycle of the RabbitMQ container:
- Starts a RabbitMQ container before tests
- Provides connection string and channel to tests
- Cleans up resources after tests complete

### TestEventConsumer

The `TestEventConsumer` class is a test helper that:
- Subscribes to specific routing keys
- Captures all received events
- Provides async waiting for specific events
- Tracks event metadata (CorrelationId, timestamp, etc.)

## Test Patterns

### Basic Event Publishing Test

```csharp
[Fact]
public async Task Event_Should_Be_Published()
{
    // Arrange
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
        })
        .Build();

    using var publisher = new ShipmentEventPublisher(configuration);
    using var consumer = new TestEventConsumer(
        _fixture.Channel!,
        "test.queue",
        "shipment.booked");

    var evt = new ShipmentBookedEvent { /* ... */ };

    // Act
    await publisher.PublishAsync(evt);

    // Assert
    var receivedEvent = await consumer.WaitForEventAsync("ShipmentBookedEvent", TimeSpan.FromSeconds(5));
    receivedEvent.Should().NotBeNull();
}
```

### Multi-Consumer Test

```csharp
[Fact]
public async Task Event_Should_Be_Consumed_By_Multiple_Services()
{
    // Arrange
    using var consumer1 = new TestEventConsumer(_fixture.Channel!, "queue1", "payment.captured");
    using var consumer2 = new TestEventConsumer(_fixture.Channel!, "queue2", "payment.captured");

    // Act
    await publisher.PublishAsync(evt);

    // Assert
    var event1 = await consumer1.WaitForEventAsync("PaymentCapturedEvent", TimeSpan.FromSeconds(5));
    var event2 = await consumer2.WaitForEventAsync("PaymentCapturedEvent", TimeSpan.FromSeconds(5));
    
    event1.Should().NotBeNull();
    event2.Should().NotBeNull();
}
```

## Troubleshooting

### Docker Not Running

If you see errors about Docker not being available:
1. Ensure Docker Desktop is running
2. Check Docker is accessible: `docker ps`
3. Restart Docker Desktop if needed

### Tests Timing Out

If tests timeout waiting for events:
1. Increase timeout in `WaitForEventAsync` calls
2. Check RabbitMQ container logs: `docker logs <container-id>`
3. Verify routing keys match between publisher and consumer

### Port Conflicts

If RabbitMQ container fails to start due to port conflicts:
1. Check if port 5672 or 15672 is already in use
2. Stop any existing RabbitMQ instances
3. Testcontainers will automatically assign random ports

### Memory Issues

If Docker runs out of memory:
1. Increase Docker Desktop memory allocation
2. Run fewer tests in parallel
3. Clean up unused Docker containers: `docker system prune`

## CI/CD Integration

These tests are designed to run in CI/CD pipelines. Ensure your CI environment:
- Has Docker available
- Has sufficient memory (minimum 2GB)
- Can pull Docker images from Docker Hub

### GitHub Actions Example

```yaml
- name: Run Integration Tests
  run: dotnet test tests/Ship24X7.IntegrationTests/Ship24X7.IntegrationTests.csproj
  env:
    DOCKER_HOST: unix:///var/run/docker.sock
```

## Performance Considerations

- Each test class uses a shared RabbitMQ container (via `IClassFixture`)
- Tests within a class run sequentially to avoid queue conflicts
- Average test execution time: 2-5 seconds per test
- Full suite execution time: ~2-3 minutes

## Future Enhancements

- [ ] Add tests for ShipmentDelayed event flow
- [ ] Add tests for PaymentFailed event flow
- [ ] Add tests for RefundProcessed event flow
- [ ] Add performance tests for high-volume event publishing
- [ ] Add tests for event schema validation
- [ ] Add tests for event versioning

## Related Documentation

- [Event Flow Testing Guide](../../src/Services/Shipment/EVENT_FLOW_TESTING.md)
- [Requirements Document](../../.kiro/specs/ship24x7-platform/requirements.md)
- [Design Document](../../.kiro/specs/ship24x7-platform/design.md)

## Support

For issues or questions about these tests, please refer to the main project documentation or contact the development team.
