using System.Collections.Concurrent;
using Navend.Core.Caching.Abstract;

namespace Navend.Core.Caching.Concrete;

public abstract class InMemoryCache<T> : IBaseCache<T>
{
    private ConcurrentDictionary<string, T> _cache = new();

    protected InMemoryCache() { }

    public T? Get(string key)
    {
        _cache.TryGetValue(key, out var value);
        return value;
    }

    public IReadOnlyDictionary<string, T> GetAll()
    {
        return _cache;
    }

    public void Set(string key, T value)
    {
        _cache[key] = value;
    }

    public Task SetAsync(string key, T value)
    {
        _cache[key] = value;
        return Task.CompletedTask;
    }

    public void Reset()
    {
        _cache.Clear();
        LoadData(); // Bu doğru mu?
    }

    protected abstract void LoadFromSource();

    protected virtual void LoadData()
    {
        LoadFromSource();
    }

    public bool TryGet(string key, out T? value)
    {
        if (IsExist(key))
        {
            value = _cache[key];
            return true;
        }
        value = default;
        return false;
    }

    public bool IsExist(string key)
    {
        return _cache.ContainsKey(key);
    }

    public Task<T?> GetAsync(string key)
    {
        _cache.TryGetValue(key, out var value);
        return Task.FromResult(value);
    }

    public Task<IReadOnlyDictionary<string, T>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyDictionary<string, T>>(_cache);
    }

    public Task<bool> TryGetAsync(string key, out T? value)
    {
        return Task.FromResult(_cache.TryGetValue(key, out value));
    }

    public Task<bool> IsExistAsync(string key)
    {
        return Task.FromResult(_cache.ContainsKey(key));
    }
}
