using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Venice.Orders.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Venice.Orders.Infrastructure.Messaging;

public class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchange = "venice.exchange";

    public RabbitMqPublisher(IConfiguration config)
    {
        var host = config.GetValue<string>("RABBITMQ_HOST") ?? "rabbitmq";
        var user = config.GetValue<string>("RABBITMQ_USER") ?? "guest";
        var pass = config.GetValue<string>("RABBITMQ_PASS") ?? "guest";

        var factory = new ConnectionFactory()
        {
            HostName = host,
            UserName = user,
            Password = pass
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclarePassive(_exchange);

    }

    public Task PublishOrderCreatedAsync(Guid orderId, CancellationToken ct = default)
    {
        var message = JsonSerializer.Serialize(new
        {
            OrderId = orderId,
            Event = "PedidoCriado",
            OccurredAt = DateTime.UtcNow
        });

        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(_exchange, "", null, body);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch { }
    }
}
