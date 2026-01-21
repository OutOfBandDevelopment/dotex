using System;

namespace OoBDev.Caching;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class IsCacheableAttribute : Attribute
{
    public IsCacheableAttribute(string keyFormatter, string lifetimeSpan) => (KeyFormatter, LifeTime) = (keyFormatter, TimeSpan.Parse(lifetimeSpan));

    public string KeyFormatter { get; }
    public TimeSpan LifeTime { get; }
}
