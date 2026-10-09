namespace OoBDev.Redis.Caching;

/// <summary>
/// Provider keys of the Redis caching adapter.
/// </summary>
public static class RedisGlobals
{
    /// <summary>
    /// The kebab-case key the Redis caching services are registered under.
    /// </summary>
    public const string ProviderKey = "redis";

    /// <summary>
    /// The earlier key, still registered so existing configuration keeps working.
    /// </summary>
    public const string LegacyKey = "Redis";
}
