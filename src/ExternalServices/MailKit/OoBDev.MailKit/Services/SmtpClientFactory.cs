using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
namespace OoBDev.MailKit.Services;

/// <summary>
/// Implementation of <see cref="ISmtpClientFactory"/> for creating instances of the SmtpClient class.
/// </summary>
/// <remark>
/// Initializes a new instance of the <see cref="SmtpClientFactory"/> class.
/// </remark>
/// <param name="config">The configuration options for the MailKit SMTP client.</param>
public class SmtpClientFactory(
    IOptions<MailKitSmtpClientOptions> config
    ) : ISmtpClientFactory
{
    /// <summary>
    /// Creates a new instance of the SmtpClient class and configures it based on the provided options.
    /// </summary>
    /// <returns>An asynchronous task that represents the creation of the SmtpClient instance.</returns>
    public async Task<ISmtpClient> CreateAsync()
    {
        var client = new SmtpClient();

        if (config.Value.Uri != null)
        {
            await client.ConnectAsync(config.Value.Uri);
        }
        else
        {
            await client.ConnectAsync(config.Value.Host, config.Value.Port, config.Value.SecureSocketOption);
        }

        if (!string.IsNullOrWhiteSpace(config.Value.UserName) && !string.IsNullOrWhiteSpace(config.Value.Password))
        {
            await client.AuthenticateAsync(config.Value.UserName, config.Value.Password);
        }

        return client;
    }
}
