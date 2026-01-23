using System.Threading.Tasks;

namespace OoBDev.System.IO;

/// <summary>
/// Represents a device that can transmit messages.
/// </summary>
/// <typeparam name="TMessage">The type of messages the device can transmit.</typeparam>
public interface IDeviceTransmitter<TMessage> : IDevice<TMessage>
{
    /// <summary>
    /// Transmits a message to the device asynchronously.
    /// </summary>
    /// <param name="message">The message to transmit.</param>
    /// <returns>A task representing the asynchronous operation, containing <c>true</c> if the transmission was successful; otherwise, <c>false</c>.</returns>
    Task<bool> Transmit(TMessage message);
}
