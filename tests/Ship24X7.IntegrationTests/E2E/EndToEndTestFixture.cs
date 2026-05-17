using Microsoft.Data.SqlClient;
using RabbitMQ.Client;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace Ship24X7.IntegrationTests.E2E;

/// <summary>
/// Test fixture for end-to-end integration tests using Testcontainers for SQL Server and RabbitMQ
/// Provides infrastructure for testing complete flows across multiple services
/// </summary>
public class EndToEndTestFixture : IAsyncLifetime
{
    private MsSqlContainer? _sqlContainer;
    private RabbitMqContainer? _rabbitMqContainer;
    
    public string SqlConnectionString { get; private set; } = string.Empty;
    public string RabbitMqConnectionString { get; private set; } = string.Empty;
    public IConnection? RabbitMqConnection { get; private set; }
    public IModel? RabbitMqChannel { get; private set; }

    public async Task InitializeAsync()
    {
        // Start SQL Server container
        _sqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Passw0rd")
            .Build();

        await _sqlContainer.StartAsync();
        SqlConnectionString = _sqlContainer.GetConnectionString();

        // Start RabbitMQ container
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .Build();

        await _rabbitMqContainer.StartAsync();
        RabbitMqConnectionString = _rabbitMqContainer.GetConnectionString();
        
        // Create RabbitMQ connection and channel
        var factory = new ConnectionFactory
        {
            Uri = new Uri(RabbitMqConnectionString)
        };
        
        RabbitMqConnection = factory.CreateConnection();
        RabbitMqChannel = RabbitMqConnection.CreateModel();
        
        // Declare exchange
        RabbitMqChannel.ExchangeDeclare(
            exchange: "ship24x7.events",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        // Initialize databases
        await InitializeDatabasesAsync();
    }

    private async Task InitializeDatabasesAsync()
    {
        // Create databases for each service
        var databases = new[] { "AuthDb", "ShipmentsDb", "TrackingDb", "NotificationsDb", "PaymentsDb" };
        
        using var connection = new SqlConnection(SqlConnectionString);
        await connection.OpenAsync();
        
        foreach (var dbName in databases)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{dbName}') CREATE DATABASE [{dbName}]";
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task DisposeAsync()
    {
        RabbitMqChannel?.Close();
        RabbitMqChannel?.Dispose();
        RabbitMqConnection?.Close();
        RabbitMqConnection?.Dispose();
        
        if (_sqlContainer != null)
        {
            await _sqlContainer.DisposeAsync();
        }
        
        if (_rabbitMqContainer != null)
        {
            await _rabbitMqContainer.DisposeAsync();
        }
    }
}
