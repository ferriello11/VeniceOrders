using MongoDB.Driver;
using Venice.Orders.Application.Interfaces;
using Venice.Orders.Domain.Entities;
using Microsoft.Extensions.Configuration;
namespace Venice.Orders.Infrastructure.Mongo;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly IMongoCollection<OrderItem> _collection;
    public OrderItemRepository(IMongoClient client, IConfiguration config)
    {
        var db = client.GetDatabase(config.GetValue<string>("Mongo:Database") ?? "venice");
        _collection = db.GetCollection<OrderItem>("orderItems");
    }

    public Task AddItemsAsync(IEnumerable<OrderItem> items, CancellationToken ct = default) =>
        _collection.InsertManyAsync(items, cancellationToken: ct);

    public async Task<List<OrderItem>> GetItemsByOrderIdAsync(Guid orderId, CancellationToken ct = default) =>
        await _collection.Find(i => i.OrderId == orderId).ToListAsync(ct);
}
