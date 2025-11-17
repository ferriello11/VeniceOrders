using Microsoft.EntityFrameworkCore;
using Venice.Orders.Domain.Entities;

namespace Venice.Orders.Infrastructure.Persistence;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }
    public DbSet<Order> Orders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().HasKey(o => o.Id);
        modelBuilder.Entity<Order>().Property(o => o.ClientId).IsRequired();
        modelBuilder.Entity<Order>().Property(o => o.CreatedAt).IsRequired();
        modelBuilder.Entity<Order>().Property(o => o.Status).IsRequired();
    }
}
