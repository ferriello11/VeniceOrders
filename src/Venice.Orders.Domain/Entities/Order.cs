namespace Venice.Orders.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Domain.Enums.OrderStatus Status { get; private set; }

    protected Order() { }

    public Order(Guid clientId)
    {
        Id = Guid.NewGuid();
        ClientId = clientId;
        CreatedAt = DateTime.UtcNow;
        Status = Domain.Enums.OrderStatus.Created;
    }

    public void SetStatus(Domain.Enums.OrderStatus status) => Status = status;
}
