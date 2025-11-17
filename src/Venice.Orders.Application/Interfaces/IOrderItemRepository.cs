using Venice.Orders.Domain.Entities;

namespace Venice.Orders.Application.Interfaces;

public interface IOrderItemRepository
{
    Task AddItemsAsync(IEnumerable<OrderItem> items, CancellationToken ct = default);
    Task<List<OrderItem>> GetItemsByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}
