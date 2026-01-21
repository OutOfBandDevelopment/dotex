using OoBDev.System.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace OoBDev.Caching;

[ContractConfig(
    AllowDefault = true,
    ConfigKey = "OoBDev:CachingProvider:Type"
    )]
public interface ICachingProvider
{
    Task FlushAsync(string key);
    Task StoreAsync(string key, object data, TimeSpan expiration);
    Task<object?> RetreiveAsync(string key, Type targetType);
}
