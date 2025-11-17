namespace Venice.Orders.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishOrderCreatedAsync(Guid orderId, CancellationToken ct = default);
}
