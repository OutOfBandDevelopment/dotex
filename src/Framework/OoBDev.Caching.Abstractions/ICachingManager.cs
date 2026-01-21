using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OoBDev.Caching;

public interface ICachingManager
{
    string BuildKey(MethodInfo method, params object[] args);

    Task FlushAsync(string key);

    Task StoreAsync(string key, object data, TimeSpan lifeTime);
    Task<object?> RetreiveAsync(string key, Type targetType);
    Task<T> RetreiveAsync<T>(string key);
}
