using System;

namespace OoBDev.Caching.Abstractions
{
    public static class ServiceProviderExtensions
    {
        public static TInterface Cacheable<TInterface, TImplemention>(this IServiceProvider serviceProvider)
            where TImplemention : class, TInterface =>
            ((ICacheableFactory)serviceProvider.GetService(typeof(ICacheableFactory))).Create<TInterface, TImplemention>()
            ?? throw new NotSupportedException();
    }
}
