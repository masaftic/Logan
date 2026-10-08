using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace IntegrationTests.Fixtures;

public class TestcontainersFixture : IAsyncLifetime
{
    public PostgreSqlContainer PostgresContainer { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("orders_test_db")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    public RabbitMqContainer RabbitMqContainer { get; } = new RabbitMqBuilder()
        .WithImage("rabbitmq:4.0-alpine")
        .WithUsername("test_guest")
        .WithPassword("test_guest")
        .Build();

    public string PostgresConnectionString => PostgresContainer.GetConnectionString();
    public string RabbitMqConnectionString => RabbitMqContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            PostgresContainer.StartAsync(),
            RabbitMqContainer.StartAsync()
        );
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            PostgresContainer.DisposeAsync().AsTask(),
            RabbitMqContainer.DisposeAsync().AsTask()
        );
    }
}
