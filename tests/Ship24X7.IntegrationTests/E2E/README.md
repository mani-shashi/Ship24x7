# End-to-End Integration Tests

This directory contains end-to-end integration tests for the Ship24X7 platform that validate complete workflows across multiple microservices.

## Overview

These tests validate complete business flows from start to finish, testing the integration of multiple services, the API Gateway, RabbitMQ message broker, and SQL Server databases.

## Test Coverage

### 1. ShipmentBookingFlowTests
Tests the complete shipment booking workflow:
- Shipment creation and booking
- Payment processing integration
- Event propagation across services
- CorrelationId tracking through the entire flow
- Rate calculation integration

**Validates Requirements:** 1.1, 2.1, 6.1, 7.1, 17.1, 17.3, 12.1

### 2. TrackingAndDeliveryFlowTests
Tests the tracking and delivery workflow:
- Tracking event recording and progression
- Pickup scheduling and completion
- Delivery proof capture with GPS coordinates
- Shipment delay notifications
- Event chain validation

**Validates Requirements:** 8.1, 9.1, 11.1, 12.2

### 3. ResilienceAndExceptionHandlingTests
Tests platform resilience patterns:
- Dead Letter Queue (DLQ) routing after failed retries
- CorrelationId preservation through retries
- Exponential backoff retry logic
- Circuit breaker behavior
- Graceful shutdown handling
- Rate limiting enforcement

**Validates Requirements:** 16.2, 16.4, 16.9, 16.10, 12.6

## Test Infrastructure

### EndToEndTestFixture
Provides complete test infrastructure using Testcontainers:
- **SQL Server**: Isolated database instance for each test run
- **RabbitMQ**: Message broker with management UI
- **Database Initialization**: Automatic creation of all service databases

### Test Patterns

#### Complete Flow Testing
```csharp
[Fact]
public async Task CompleteShipmentBookingFlow_ShouldPublishEventsInCorrectOrder()
{
    // Arrange - Set up publishers and consumers
    // Act - Execute complete workflow
    // Assert - Verify all events and state transitions
}
```

#### Multi-Service Integration
```csharp
[Fact]
public async Task DeliveryProofCapture_ShouldPublishShipmentDeliveredEvent()
{
    // Tests integration between Tracking Service and Notification Service
}
```

## Prerequisites

- .NET 10.0 SDK
- Docker Desktop (for Testcontainers)
- At least 4GB of available RAM for Docker containers
- Sufficient disk space for SQL Server and RabbitMQ images

## Running the Tests

### Run All E2E Tests

```bash
dotnet test tests/Ship24X7.IntegrationTests/Ship24X7.IntegrationTests.csproj --filter "FullyQualifiedName~E2E"
```

### Run Specific Test Class

```bash
# Shipment booking flow tests
dotnet test tests/Ship24X7.IntegrationTests/Ship24X7.IntegrationTests.csproj --filter "FullyQualifiedName~ShipmentBookingFlowTests"

# Tracking and delivery flow tests
dotnet test tests/Ship24X7.IntegrationTests/Ship24X7.IntegrationTests.csproj --filter "FullyQualifiedName~TrackingAndDeliveryFlowTests"

# Resilience tests
dotnet test tests/Ship24X7.IntegrationTests/Ship24X7.IntegrationTests.csproj --filter "FullyQualifiedName~ResilienceAndExceptionHandlingTests"
```

### Run with Verbose Output

```bash
dotnet test tests/Ship24X7.IntegrationTests/Ship24X7.IntegrationTests.csproj --filter "FullyQualifiedName~E2E" --logger "console;verbosity=detailed"
```

## Test Architecture

### Container Management
- Each test class uses `IClassFixture<RabbitMqTestFixture>` for shared RabbitMQ container
- SQL Server container is managed by `EndToEndTestFixture` (when needed)
- Containers are automatically started before tests and cleaned up after

### Event Flow Validation
- Tests use `TestEventConsumer` to capture events from RabbitMQ
- CorrelationId is tracked through all service interactions
- Event ordering and timing are validated

### Database Isolation
- Each test run gets fresh database instances
- No test data pollution between runs
- Automatic schema creation and cleanup

## Performance Considerations

- **Container Startup**: First test run takes 30-60 seconds for container initialization
- **Subsequent Tests**: Run in 2-5 seconds each after containers are up
- **Full Suite**: Approximately 3-5 minutes for all E2E tests
- **Parallel Execution**: Tests within a class run sequentially to avoid conflicts

## Troubleshooting

### Docker Not Running
```bash
# Check Docker status
docker ps

# Start Docker Desktop if needed
```

### Container Startup Failures
```bash
# Check available resources
docker system df

# Clean up unused containers
docker system prune -a
```

### Port Conflicts
Testcontainers automatically assigns random ports, but if you see conflicts:
```bash
# Check what's using ports
netstat -an | grep 5672
netstat -an | grep 1433

# Stop conflicting services
docker stop $(docker ps -q)
```

### Memory Issues
If tests fail due to insufficient memory:
1. Increase Docker Desktop memory allocation (Settings → Resources)
2. Close other applications
3. Run fewer tests in parallel

### Test Timeouts
If tests timeout waiting for events:
1. Check RabbitMQ container logs: `docker logs <container-id>`
2. Verify routing keys match between publisher and consumer
3. Increase timeout in `WaitForEventAsync` calls

## CI/CD Integration

These tests are designed for CI/CD pipelines:

### GitHub Actions Example
```yaml
name: E2E Tests

on: [push, pull_request]

jobs:
  e2e-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
    
    - name: Run E2E Tests
      run: dotnet test tests/Ship24X7.IntegrationTests/Ship24X7.IntegrationTests.csproj --filter "FullyQualifiedName~E2E"
      env:
        DOCKER_HOST: unix:///var/run/docker.sock
```

### Azure DevOps Example
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run E2E Tests'
  inputs:
    command: 'test'
    projects: 'tests/Ship24X7.IntegrationTests/Ship24X7.IntegrationTests.csproj'
    arguments: '--filter "FullyQualifiedName~E2E"'
```

## What's NOT Covered

These E2E tests focus on event-driven integration and do NOT cover:
- Frontend UI testing (use Playwright/Cypress for that)
- API Gateway HTTP routing (covered in Gateway integration tests)
- Individual service unit tests (covered in service-specific test projects)
- Performance/load testing (use dedicated performance test tools)

## Future Enhancements

- [ ] Add admin dashboard flow tests (Requirement 13.1, 13.2)
- [ ] Add user management flow tests (Requirement 14.1)
- [ ] Add hub management flow tests (Requirement 15.1)
- [ ] Add OAuth login flow tests (Requirement 4.1)
- [ ] Add MFA enrollment flow tests (Requirement 3.1)
- [ ] Add refund processing flow tests (Requirement 17.10)
- [ ] Add performance benchmarks for event throughput
- [ ] Add chaos engineering tests (random service failures)

## Related Documentation

- [Integration Tests README](../README.md)
- [Requirements Document](../../../.kiro/specs/ship24x7-platform/requirements.md)
- [Design Document](../../../.kiro/specs/ship24x7-platform/design.md)
- [Tasks Document](../../../.kiro/specs/ship24x7-platform/tasks.md)

## Support

For issues or questions about these tests:
1. Check the troubleshooting section above
2. Review the main integration tests documentation
3. Check Docker and Testcontainers documentation
4. Contact the development team

