using System.Threading.Tasks;

namespace OoBDev.Communications.Services;

/// <summary>
/// Represents an interface for sending communication messages of a specified type.
/// </summary>
/// <typeparam name="TMessageType">The type of the communication message.</typeparam>
public interface ICommunicationSender<TMessageType>
{
    /// <summary>
    /// Asynchronously sends the specified communication message.
    /// </summary>
    /// <param name="message">The communication message to send.</param>
    /// <returns>A task representing the asynchronous operation. The result is the unique identifier associated with the sent message.</returns>
    Task<string> SendAsync(TMessageType message);
}
