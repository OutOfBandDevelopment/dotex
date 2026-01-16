using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Communications
{
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddCommunicationsServices(this IServiceCollection services)=>
            new CommunicationsRegistrar().AddServices(services);
    }
}
