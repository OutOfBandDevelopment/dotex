using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.ComplexEvents.Common
{
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddComplexEventsServices(this IServiceCollection services)=>
            new ComplexEventsRegistrar().AddServices(services);
    }
}
