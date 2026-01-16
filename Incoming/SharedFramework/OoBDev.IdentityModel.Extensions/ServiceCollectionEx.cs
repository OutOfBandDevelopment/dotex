using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.IdentityModel.Extensions
{
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddIdentityModelExtensions(this IServiceCollection services) =>
            new IdentityModelRegistrar().AddServices(services);
    }
}
