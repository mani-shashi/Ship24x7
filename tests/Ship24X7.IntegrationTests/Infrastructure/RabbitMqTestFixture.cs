using RabbitMQ.Client;
using Testcontainers.RabbitMq;
using Xunit;

namespace Ship24X7.IntegrationTests.Infrastructure;

/// <summary>
/// Test fixture for RabbitMQ integration tests using Testcontainers
/// </summary>
public class RabbitMqTestFixture : IAsyncLifetime
{
    private RabbitMqContainer? _rabbitMqContainer;
    
    public string ConnectionString { get; private set; } = string.Empty;
    public IConnection? Connection { get; private set; }
    public IModel? Channel { get; private set; }

    public async Task InitializeAsync()
    {
        // Start RabbitMQ container with proxy bypass to avoid network issues
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .WithEnvironment("HTTP_PROXY", "")
            .WithEnvironment("HTTPS_PROXY", "")
            .WithEnvironment("NO_PROXY", "*")
            .Build();

        await _rabbitMqContainer.StartAsync();
        
        ConnectionString = _rabbitMqContainer.GetConnectionString();
        
        // Create connection and channel
        var factory = new ConnectionFactory
        {
            Uri = new Uri(ConnectionString)
        };
        
        Connection = factory.CreateConnection();
        Channel = Connection.CreateModel();
        
        // Declare exchange
        Channel.ExchangeDeclare(
            exchange: "ship24x7.events",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);
    }

    public async Task DisposeAsync()
    {
        Channel?.Close();
        Channel?.Dispose();
        Connection?.Close();
        Connection?.Dispose();
        
        if (_rabbitMqContainer != null)
        {
            await _rabbitMqContainer.DisposeAsync();
        }
    }
    
    public void PurgeQueue(string queueName)
    {
        try
        {
            Channel?.QueuePurge(queueName);
        }
        catch
        {
            // Queue might not exist yet
        }
    }
}
