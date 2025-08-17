namespace Navend.Core.Constants;
[Flags]
public enum CacheTypes
{
    None = 0,
    InMemory     = 1 << 0, // 1 2^0
    Redis        = 1 << 1, // 2 2^1
}
