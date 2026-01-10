using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace OoBDev.Api.Redis.Caching.Providers
{
    public class ConnectionMultiplexerFactory : IConnectionMultiplexerFactory
    {
        public const string SourceConfigurationKey = "Redis:ConnectionMultiplexer:Config";

        private readonly IConfiguration _config;

        public ConnectionMultiplexerFactory(
            IConfiguration config
            ) => _config = config;

        public IConnectionMultiplexer Create() => ConnectionMultiplexer.Connect(_config[SourceConfigurationKey]);
    }
}
