using MailKit.Net.Imap;
using System.Threading.Tasks;

namespace OoBDev.MailKit.Services;

/// <summary>
/// Represents a factory for creating instances of <see cref="ImapClient"/>.
/// </summary>
public interface IImapClientFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="ImapClient"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation and contains the created <see cref="ImapClient"/>.</returns>
    Task<IImapClient> CreateAsync();
}
