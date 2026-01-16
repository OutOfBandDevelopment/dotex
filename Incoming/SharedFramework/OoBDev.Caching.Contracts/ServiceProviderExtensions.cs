using System;

namespace OoBDev.Caching.Contracts
{
    public static class ServiceProviderExtensions
    {
        public static TInterface Cacheable<TInterface, TImplemention>(this IServiceProvider serviceProvider)
            where TImplemention : class, TInterface =>
            ((ICacheableFactory)serviceProvider.GetService(typeof(ICacheableFactory))).Create<TInterface, TImplemention>()
            ?? throw new NotSupportedException();
    }
}
