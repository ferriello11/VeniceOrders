using StackExchange.Redis;
using System.Text.Json;

namespace Venice.Orders.Infrastructure.Cache;

public class RedisCacheService
{
    private readonly IDatabase _db;
    public RedisCacheService(IConnectionMultiplexer multiplexer) => _db = multiplexer.GetDatabase();

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, ttl);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var val = await _db.StringGetAsync(key);
        if (!val.HasValue) return default;
        return JsonSerializer.Deserialize<T>(val!);
    }
}
