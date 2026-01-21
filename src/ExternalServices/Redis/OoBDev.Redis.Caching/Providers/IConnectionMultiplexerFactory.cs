using StackExchange.Redis;

namespace OoBDev.Redis.Caching.Providers;

/// <summary>
/// Factory interface for creating Redis connection multiplexer instances.
/// </summary>
public interface IConnectionMultiplexerFactory
{
    /// <summary>
    /// Creates a new Redis connection multiplexer instance.
    /// </summary>
    /// <returns>A configured <see cref="IConnectionMultiplexer"/> instance.</returns>
    IConnectionMultiplexer Create();
}
