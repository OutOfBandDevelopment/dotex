using OoBDev.Twilio.SmsMessaging.Communications;
using OoBDev.Twilio.SmsMessaging.Shared;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Twilio.SmsMessaging
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class SmsMessagingRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.AddTransient<ISendSmsHandler, SendTwilioSmsHandler>();
            services.TryAddTransient<ISmsClient, SmsClient>();
            return services;
        }
    }
}
