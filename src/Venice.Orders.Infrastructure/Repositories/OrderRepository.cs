using Venice.Orders.Application.Interfaces;
using Venice.Orders.Domain.Entities;
using Venice.Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Venice.Orders.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _db;
    public OrderRepository(OrdersDbContext db) => _db = db;

    public async Task AddAsync(Order order, CancellationToken ct = default) => await _db.Orders.AddAsync(order, ct);

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
