using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Navend.Core.Caching.Abstract;

namespace Navend.Core.Caching.Concrete;

public class CacheWarmUpHostedService : IHostedService
{
    private readonly IEnumerable<ICacheWarmUpService> _cacheWarmers;
    private readonly ILogger<CacheWarmUpHostedService> _logger;

    public CacheWarmUpHostedService(IEnumerable<ICacheWarmUpService> cacheWarmers, ILogger<CacheWarmUpHostedService> logger)
    {
        _cacheWarmers = cacheWarmers;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var cache in _cacheWarmers)
        {
            try
            {
                _logger.LogInformation("Warming up cache: {CacheName}", cache.CacheName);
                await cache.LoadAsync();
                _logger.LogInformation("Cache {CacheName} warmed up successfully.", cache.CacheName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up cache: {CacheName}", cache.CacheName);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}