using StackExchange.Redis;

namespace OoBDev.Redis.Caching.Providers;

public interface IConnectionMultiplexerFactory
{
    IConnectionMultiplexer Create();
}
