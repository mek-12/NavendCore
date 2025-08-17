using StackExchange.Redis;
using Navend.Core.Caching.Abstract;
using System.Text.Json;

namespace Navend.Core.Caching.Concrete;

public class RedisCache<T> : IRedisCache<T>
{
    private readonly IDatabase _db;
    private readonly string _ns;            // namespace/prefix, e.g. "yourapp:products:"
    private readonly string _keyIndex;      // yourapp:products:__keys
    private readonly JsonSerializerOptions _json;

    public RedisCache(
        IConnectionMultiplexer mux,
        string @namespace,
        JsonSerializerOptions? jsonOptions = null)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
            throw new ArgumentException("Namespace must not be empty.", nameof(@namespace));

        _db = mux.GetDatabase();
        _ns = @namespace.EndsWith(":") ? @namespace : @namespace + ":";
        _keyIndex = _ns + "__keys";
        _json = jsonOptions ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    private string K(string key) => _ns + key;

    // ---------- GET ----------
    public T? Get(string key)
    {
        var val = _db.StringGet(K(key));
        if (val.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(val!, _json);
    }

    public async Task<T?> GetAsync(string key)
    {
        var val = await _db.StringGetAsync(K(key)).ConfigureAwait(false);
        if (val.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(val!, _json);
    }

    // ---------- GET ALL ----------
    public IReadOnlyDictionary<string, T> GetAll()
    {
        // fetch all logical keys from the index set
        var keys = _db.SetMembers(_keyIndex);
        if (keys.Length == 0) return new Dictionary<string, T>();

        // bulk get with MGET
        var redisKeys = keys.Select(k => (RedisKey)K(k!)).ToArray();
        var values = _db.StringGet(redisKeys);

        var dict = new Dictionary<string, T>(keys.Length);
        for (int i = 0; i < keys.Length; i++)
        {
            var key = (string)keys[i]!;
            var val = values[i];
            if (val.HasValue)
            {
                var obj = JsonSerializer.Deserialize<T>(val!, _json);
                if (obj is not null)
                    dict[key] = obj;
            }
        }
        return dict;
    }

    public async Task<IReadOnlyDictionary<string, T>> GetAllAsync()
    {
        var keys = await _db.SetMembersAsync(_keyIndex).ConfigureAwait(false);
        if (keys.Length == 0) return new Dictionary<string, T>();

        var redisKeys = keys.Select(k => (RedisKey)K(k!)).ToArray();
        var values = await _db.StringGetAsync(redisKeys).ConfigureAwait(false);

        var dict = new Dictionary<string, T>(keys.Length);
        for (int i = 0; i < keys.Length; i++)
        {
            var key = (string)keys[i]!;
            var val = values[i];
            if (val.HasValue)
            {
                var obj = JsonSerializer.Deserialize<T>(val!, _json);
                if (obj is not null)
                    dict[key] = obj;
            }
        }
        return dict;
    }

    // ---------- TRY GET ----------
    public bool TryGet(string key, out T? value)
    {
        var val = _db.StringGet(K(key));
        if (val.IsNullOrEmpty)
        {
            value = default;
            return false;
        }
        value = JsonSerializer.Deserialize<T>(val!, _json);
        return value is not null;
    }

    // Note: cannot be `async` because of the `out` parameter; this is fine/valid.
    public Task<bool> TryGetAsync(string key, out T? value)
    {
        var ok = TryGet(key, out value);
        return Task.FromResult(ok);
    }

    // ---------- EXISTS ----------
    public bool IsExist(string key) => _db.KeyExists(K(key));

    public Task<bool> IsExistAsync(string key) => _db.KeyExistsAsync(K(key));

    // ---------- SET ----------
    public void Set(string key, T value)
    {
        var payload = JsonSerializer.Serialize(value, _json);
        _db.StringSet(K(key), payload);
        _db.SetAdd(_keyIndex, key); // track logical key
    }

    public async Task SetAsync(string key, T value)
    {
        var payload = JsonSerializer.Serialize(value, _json);
        await _db.StringSetAsync(K(key), payload).ConfigureAwait(false);
        await _db.SetAddAsync(_keyIndex, key).ConfigureAwait(false);
    }

    // ---------- RESET (delete everything under namespace) ----------
    public void Reset()
    {
        var keys = _db.SetMembers(_keyIndex);
        if (keys.Length > 0)
        {
            var delKeys = keys.Select(k => (RedisKey)K(k!)).ToArray();
            _db.KeyDelete(delKeys);
        }
        _db.KeyDelete(_keyIndex);
    }
}