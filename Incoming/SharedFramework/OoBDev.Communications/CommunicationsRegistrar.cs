using OoBDev.Communications.Composers;
using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.Composers;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Contracts.Services;
using OoBDev.Communications.Handler;
using OoBDev.Communications.Provider;
using OoBDev.Communications.Services;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Communications
{
    public class CommunicationsRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.TryAddTransient<ICommunicationProvider, CommunicationProvider>();
            services.TryAddTransient<IAttributeResolver, AttributeResolver>();

            services.TryAddTransient<ICommunicationCentralProvider, CommunicationCentralProvider>();
            services.TryAddTransient<ICommunicationCentralProcessor, CommunicationCentralProcessor>();
            services.TryAddTransient<ITargetPreferenceManager, TargetPreferenceManager>();
            services.TryAddTransient<IDataEnhancementManager, DataEnhancementManager>();
            services.TryAddSingleton<IDataEnhancementProviderFactory, DataEnhancementProviderFactory>();
            services.TryAddTransient<IMessageComposerFactory, MessageComposerFactory>();

            services.AddTransient<IMessageComposer, EmailMessageComposer>();
            services.TryAddTransient<IEmailMessageComposerConfig, EmailMessageComposerConfig>();
            services.AddTransient<IMessageComposer, SmsMessageComposer>();
            services.AddTransient<IMessageComposer, NoMessageComposer>();

            services.TryAddTransient<ITemplateProvider, TemplateProvider>();
            services.TryAddTransient<ICommunicationDeferralProvider, CommunicationDeferralProvider>();
            services.TryAddTransient<ICommunicationDeferralConfig, CommunicationDeferralConfig>();

            services.TryAddTransient<ISendSmsProvider, SendSmsProvider>();
            services.TryAddTransient<ISendEmailProvider, SendEmailProvider>();

            return services;
        }
    }
}
