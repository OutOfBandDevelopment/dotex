using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Microsoft.Caching
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddMicrosoftCachingServices(this IServiceCollection services) =>
            new MicrosoftCachingRegistrar().AddServices(services);
    }
}
