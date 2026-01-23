using System;

namespace OoBDev.Caching;

/// <summary>
/// Attribute to mark methods whose results should be cached.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class IsCacheableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IsCacheableAttribute"/> class.
    /// </summary>
    /// <param name="keyFormatter">The cache key formatter string.</param>
    /// <param name="lifetimeSpan">The cache entry lifetime as a parseable TimeSpan string (e.g., "00:05:00" for 5 minutes).</param>
    public IsCacheableAttribute(string keyFormatter, string lifetimeSpan) => (KeyFormatter, LifeTime) = (keyFormatter, TimeSpan.Parse(lifetimeSpan));

    /// <summary>
    /// Gets the cache key formatter string.
    /// </summary>
    public string KeyFormatter { get; }

    /// <summary>
    /// Gets the cache entry lifetime.
    /// </summary>
    public TimeSpan LifeTime { get; }
}
