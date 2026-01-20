using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Microsoft.BingMaps
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddMicrosoftBingMapsServices(this IServiceCollection services) =>
            new MicrosoftBingMapsRegistrar().AddServices(services);
    }
}
