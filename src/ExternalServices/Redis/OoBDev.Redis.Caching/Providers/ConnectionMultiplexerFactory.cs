using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace OoBDev.Redis.Caching.Providers;

/// <summary>
/// Factory for creating Redis connection multiplexer instances from configuration.
/// </summary>
public class ConnectionMultiplexerFactory : IConnectionMultiplexerFactory
{
    /// <summary>
    /// Configuration key for the Redis connection string.
    /// </summary>
    public const string SourceConfigurationKey = "Redis:ConnectionMultiplexer:Config";

    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionMultiplexerFactory"/> class.
    /// </summary>
    /// <param name="config">The configuration containing Redis connection settings.</param>
    public ConnectionMultiplexerFactory(
        IConfiguration config
        ) => _config = config;

    /// <inheritdoc/>
    public IConnectionMultiplexer Create() => ConnectionMultiplexer.Connect(_config[SourceConfigurationKey]);
}
