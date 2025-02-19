using OoBDev.Communications.Models;
using OoBDev.Communications.Services;
using OoBDev.MailKit.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.MailKit;

/// <summary>
/// Extensions for adding MailKit services to the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add MailKit extensions to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to which MailKit services will be added.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="smtpConfigurationSection">The configuration section name for MailKit SMTP options.</param>
    /// <param name="imapConfigurationSection">The configuration section name for MailKit IMAP options.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddMailKitExtensions(this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string smtpConfigurationSection,
        string imapConfigurationSection
#else
        string smtpConfigurationSection = nameof(MailKitSmtpClientOptions),
        string imapConfigurationSection = nameof(MailKitImapClientOptions)
#endif
        )
    {
        var smtp = configuration.GetSection(smtpConfigurationSection)?[nameof(MailKitSmtpClientOptions.Uri)] ??
                   configuration.GetSection(smtpConfigurationSection)?[nameof(MailKitSmtpClientOptions.Host)];

        var imap = configuration.GetSection(imapConfigurationSection)?[nameof(MailKitImapClientOptions.Uri)] ??
                   configuration.GetSection(imapConfigurationSection)?[nameof(MailKitImapClientOptions.Host)];

        if (string.IsNullOrWhiteSpace(smtp) && string.IsNullOrWhiteSpace(imap))
        {
            return services;
        }

        services.TryAddTransient<IMimeMessageFactory, MimeMessageFactory>();

        if (!string.IsNullOrWhiteSpace(smtp))
        {
            services.AddHealthChecks().AddCheck<MailkitSmtpHealthCheck>("mailkit-smtp");
            services.TryAddTransient<ICommunicationSender<EmailMessageModel>, MailKitProvider>();
            services.TryAddTransient<ISmtpClientFactory, SmtpClientFactory>();
            services.Configure<MailKitSmtpClientOptions>(options => configuration.Bind(smtpConfigurationSection, options));
        }

        if (!string.IsNullOrWhiteSpace(imap))
        {
            services.AddHealthChecks().AddCheck<MailkitImapHealthCheck>("mailkit-imap");
            services.TryAddTransient<IImapClientFactory, ImapClientFactory>();
            services.Configure<MailKitImapClientOptions>(options => configuration.Bind(imapConfigurationSection, options));
        }

        return services;
    }
}
