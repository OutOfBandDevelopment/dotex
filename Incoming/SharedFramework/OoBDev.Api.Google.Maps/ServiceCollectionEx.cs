using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Google.Maps
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddGoogleMapsServices(this IServiceCollection services) =>
            new GoogleMapsRegistrar().AddServices(services);
    }
}
