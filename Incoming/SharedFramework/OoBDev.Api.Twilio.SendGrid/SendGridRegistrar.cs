using OoBDev.Api.Twilio.SendGrid.Communications;
using OoBDev.Api.Twilio.SendGrid.Shared;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Twilio.SendGrid
{
    [ExcludeFromCodeCoverage]
    public class SendGridRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.AddTransient<ISendEmailHandler, SendGridEmailHandler>();
            services.TryAddTransient<IEmailMessageMapper, EmailMessageMapper>();
            services.TryAddTransient<IEmailClient, EmailClient>();
            services.TryAddTransient<IMessageBuilder, MessageBuilder>();
            return services;
        }
    }
}
