using Venice.Orders.Application.DTOs;
using Venice.Orders.Application.Interfaces;
using Venice.Orders.Domain.Entities;

namespace Venice.Orders.Application.Services;

public class OrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IOrderItemRepository _itemRepo;
    private readonly IEventPublisher _publisher;

    public OrderService(IOrderRepository orderRepo, IOrderItemRepository itemRepo, IEventPublisher publisher)
    {
        _orderRepo = orderRepo;
        _itemRepo = itemRepo;
        _publisher = publisher;
    }

    public async Task<Guid> CreateOrderAsync(CreateOrderRequest req, CancellationToken ct = default)
    {
        if (req.Items == null || !req.Items.Any())
            throw new ArgumentException("Order must contain at least one item.");

        var order = new Order(req.ClientId);
        await _orderRepo.AddAsync(order, ct);

        var items = req.Items.Select(i => new OrderItem
        {
            OrderId = order.Id,
            Product = i.Product,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList();

        await _itemRepo.AddItemsAsync(items, ct);
        await _orderRepo.SaveChangesAsync(ct);

        await _publisher.PublishOrderCreatedAsync(order.Id, ct);

        return order.Id;
    }

    public async Task<OrderDto?> GetOrderAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(id, ct);
        if (order == null) return null;

        var items = await _itemRepo.GetItemsByOrderIdAsync(id, ct);
        return new OrderDto
        {
            Id = order.Id,
            ClientId = order.ClientId,
            CreatedAt = order.CreatedAt,
            Status = order.Status.ToString(),
            Items = items.Select(it => new OrderItemDto
            {
                Product = it.Product,
                Quantity = it.Quantity,
                UnitPrice = it.UnitPrice
            }).ToList()
        };
    }

    public class OrderDto
    {
        public Guid Id { get; init; }
        public Guid ClientId { get; init; }
        public DateTime CreatedAt { get; init; }
        public string Status { get; init; } = null!;
        public List<OrderItemDto> Items { get; init; } = new();
    }

    public class OrderItemDto
    {
        public string Product { get; init; } = null!;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}
