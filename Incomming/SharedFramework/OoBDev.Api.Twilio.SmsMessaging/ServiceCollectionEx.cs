using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Twilio.SmsMessaging
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddTwilioSmsServices(this IServiceCollection services) =>
            new SmsMessagingRegistrar().AddServices(services);
    }
}
