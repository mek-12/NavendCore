namespace Navend.Core.Caching.Abstract;

public interface IBaseCache<T>
{
    T? Get(string key);
    Task<T?> GetAsync(string key);
    IReadOnlyDictionary<string, T> GetAll();
    Task<IReadOnlyDictionary<string, T>> GetAllAsync();
    bool TryGet(string key, out T? value);
    Task<bool> TryGetAsync(string key, out T? value);
    bool IsExist(string key);
    Task<bool> IsExistAsync(string key);
    void Set(string key, T value);
    Task SetAsync(string key, T value);
    void Reset();
}