using MailKit.Net.Smtp;
using System.Threading.Tasks;

namespace OoBDev.MailKit.Services;

/// <summary>
/// Represents a factory for creating instances of <see cref="SmtpClient"/>.
/// </summary>
public interface ISmtpClientFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="SmtpClient"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation and contains the created <see cref="SmtpClient"/>.</returns>
    Task<ISmtpClient> CreateAsync();
}
