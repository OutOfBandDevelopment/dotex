using StackExchange.Redis;

namespace OoBDev.Api.Redis.Caching.Providers
{
    public interface IConnectionMultiplexerFactory
    {
        IConnectionMultiplexer Create();
    }
}
