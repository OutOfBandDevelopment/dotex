using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Amazon.Sqs
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddAmazonSqsServices(this IServiceCollection services) =>
            new AmazonSqsRegistrar().AddServices(services);
    }
}
