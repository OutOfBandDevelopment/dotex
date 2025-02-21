using MailKit.Net.Imap;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace OoBDev.MailKit.Services;

/// <summary>
/// Implementation of <see cref="IImapClientFactory"/> for creating instances of the ImapClient class.
/// </summary>
/// <remark>
/// Initializes a new instance of the <see cref="ImapClientFactory"/> class.
/// </remark>
/// <param name="config">The configuration options for the MailKit Imap client.</param>
public class ImapClientFactory(
    IOptions<MailKitImapClientOptions> config
    ) : IImapClientFactory
{
    /// <summary>
    /// Creates a new instance of the ImapClient class and configures it based on the provided options.
    /// </summary>
    /// <returns>An asynchronous task that represents the creation of the ImapClient instance.</returns>
    public async Task<IImapClient> CreateAsync()
    {
        var client = new ImapClient();

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
