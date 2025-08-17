namespace Navend.Core.Caching.Abstract;

public interface ICacheWarmUpService
{
    string CacheName { get; }
    Task LoadAsync();
}
