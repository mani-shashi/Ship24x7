using System.Text;
using RabbitMQ.Client;
using Ship24X7.IntegrationTests.Infrastructure;
using Xunit;

namespace Ship24X7.IntegrationTests.EventFlows;

public class DebugTest : IClassFixture<RabbitMqTestFixture>
{
    private readonly RabbitMqTestFixture _fixture;

    public DebugTest(RabbitMqTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Direct_Publish_Should_Work()
    {
        // Arrange
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.debug.queue",
            "test.routing.key");

        // Act - Publish directly using the fixture's channel
        var properties = _fixture.Channel!.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Headers = new Dictionary<string, object>
        {
            { "EventType", "TestEvent" },
            { "CorrelationId", "test-correlation-id" }
        };

        var message = "{\"test\": \"data\"}";
        var body = Encoding.UTF8.GetBytes(message);

        _fixture.Channel!.BasicPublish(
            exchange: "ship24x7.events",
            routingKey: "test.routing.key",
            basicProperties: properties,
            body: body);

        // Assert
        await Task.Delay(1000); // Wait for message to be consumed
        
        Console.WriteLine($"Received events count: {consumer.ReceivedEvents.Count}");
        foreach (var evt in consumer.ReceivedEvents)
        {
            Console.WriteLine($"Event: Type={evt.EventType}, CorrelationId={evt.CorrelationId}, RoutingKey={evt.RoutingKey}");
        }
        
        Assert.True(consumer.ReceivedEvents.Count > 0, "Should have received at least one event");
    }
}
