using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Reacts to an inbound message; handlers run in registration order.</summary>
public interface IMessageReceivedHandler
{
    /// <summary>Handles the message.</summary>
    Task HandleAsync(IMessage message, CancellationToken cancellationToken = default);
}
