using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Venice.Orders.Application.Services;
using Venice.Orders.Application.DTOs;
using Venice.Orders.Infrastructure.Cache;

namespace Venice.Orders.Api.Controllers;

[ApiController]
[Route("pedidos")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _service;
    private readonly RedisCacheService _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(2);

    public OrdersController(OrderService service, RedisCacheService cache)
    {
        _service = service;
        _cache = cache;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        var id = await _service.CreateOrderAsync(req, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var cacheKey = $"order:{id}";
        var cached = await _cache.GetAsync<OrderService.OrderDto>(cacheKey);
        if (cached != null) return Ok(cached);

        var result = await _service.GetOrderAsync(id, ct);
        if (result == null) return NotFound();
        await _cache.SetAsync(cacheKey, result, CacheTtl);
        return Ok(result);
    }
}
