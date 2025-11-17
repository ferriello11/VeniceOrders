using Moq;
using Venice.Orders.Application.Interfaces;
using Venice.Orders.Application.Services;
using Venice.Orders.Domain.Entities;
using Venice.Orders.Application.DTOs;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_PublishesEventAndSaves()
    {
        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<System.Threading.CancellationToken>())).Returns(Task.CompletedTask);
        orderRepo.Setup(r => r.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>())).Returns(Task.CompletedTask);

        var itemRepo = new Mock<IOrderItemRepository>();
        itemRepo.Setup(r => r.AddItemsAsync(It.IsAny<IEnumerable<OrderItem>>(), It.IsAny<System.Threading.CancellationToken>())).Returns(Task.CompletedTask);

        var publisher = new Mock<IEventPublisher>();
        publisher.Setup(p => p.PublishOrderCreatedAsync(It.IsAny<Guid>(), It.IsAny<System.Threading.CancellationToken>())).Returns(Task.CompletedTask);

        var svc = new OrderService(orderRepo.Object, itemRepo.Object, publisher.Object);
        var req = new CreateOrderRequest { ClientId = Guid.NewGuid(), Items = new List<CreateOrderRequest.CreateOrderItemDto> { new() { Product = "X", Quantity = 1, UnitPrice = 10 } } };

        var id = await svc.CreateOrderAsync(req);

        orderRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        itemRepo.Verify(r => r.AddItemsAsync(It.IsAny<IEnumerable<OrderItem>>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        publisher.Verify(p => p.PublishOrderCreatedAsync(It.Is<Guid>(g => g == id), It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrder_ReturnsNullWhenNotFound()
    {
        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync((Order?)null);

        var itemRepo = new Mock<IOrderItemRepository>();
        var publisher = new Mock<IEventPublisher>();
        var svc = new OrderService(orderRepo.Object, itemRepo.Object, publisher.Object);

        var result = await svc.GetOrderAsync(Guid.NewGuid());
        Assert.Null(result);
    }
}
