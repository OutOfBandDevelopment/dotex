using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Api.Twilio.SendGrid
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddSendGridServices(this IServiceCollection services)
        {
            return new SendGridRegistrar().AddServices(services);
        }
    }
}
