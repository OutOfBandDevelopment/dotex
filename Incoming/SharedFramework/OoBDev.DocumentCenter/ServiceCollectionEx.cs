using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.DocumentCenter
{
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddDocumentCenterServices(this IServiceCollection services)=>
            new DocumentCenterRegistrar().AddServices(services);
    }
}
