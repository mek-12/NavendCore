
namespace Navend.Core.Caching.Concrete;

internal sealed class RedisOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string Password { get; set; } = "YourStrongPass";
    public string InstanceName { get; set; } = "app:";
    public int Database { get; set; } = 0;
}