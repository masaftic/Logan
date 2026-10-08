using FluentAssertions;
using IntegrationTests.Fixtures;
using Npgsql;
using RabbitMQ.Client;
using Xunit;

namespace IntegrationTests.Integration;

public class TestcontainersSanityTests : IClassFixture<TestcontainersFixture>
{
    private readonly TestcontainersFixture _fixture;

    public TestcontainersSanityTests(TestcontainersFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PostgresContainer_Should_Accept_Connections_And_Execute_Query()
    {
        // Arrange
        var connectionString = _fixture.PostgresConnectionString;
        connectionString.Should().NotBeNullOrWhiteSpace();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT 1;", connection);
        var result = await cmd.ExecuteScalarAsync();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task RabbitMqContainer_Should_Accept_Amqp_Connections()
    {
        // Arrange
        var connectionString = _fixture.RabbitMqConnectionString;
        connectionString.Should().NotBeNullOrWhiteSpace();

        // Act
        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString)
        };

        using var connection = await factory.CreateConnectionAsync();

        // Assert
        connection.IsOpen.Should().BeTrue();
    }
}
