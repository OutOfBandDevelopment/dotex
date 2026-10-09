namespace OoBDev.Microsoft.Caching;

/// <summary>
/// Provider keys of the in-memory caching adapter.
/// </summary>
public static class MicrosoftCachingGlobals
{
    /// <summary>
    /// The kebab-case key the in-memory caching services are registered under.
    /// </summary>
    public const string ProviderKey = "memory-cache";

    /// <summary>
    /// The earlier key, still registered so existing configuration keeps working.
    /// </summary>
    public const string LegacyKey = "MemoryCache";
}
