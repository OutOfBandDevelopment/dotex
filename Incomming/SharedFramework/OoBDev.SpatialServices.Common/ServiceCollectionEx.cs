using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.SpatialServices.Common
{
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddSpatialServices(this IServiceCollection services) =>
            new SpatialServicesRegistrar().AddServices(services);
    }
}
