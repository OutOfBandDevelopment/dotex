using System;
using System.Threading.Tasks;

namespace OoBDev.System.IO;

/// <summary>
/// Represents a streaming device that can both send and receive messages with status monitoring.
/// </summary>
/// <typeparam name="TMessage">The type of messages the device handles.</typeparam>
public interface IStreamDevice<TMessage> : IDeviceReceiver<TMessage>, IDeviceTransmitter<TMessage>, IDisposable
{
    /// <summary>
    /// Gets the background task that runs the device's message processing loop.
    /// </summary>
    Task Runner { get; }

    /// <summary>
    /// Occurs when a message is received from the device.
    /// </summary>
    new event EventHandler<TMessage> MessageReceived;

    /// <summary>
    /// Occurs when the device status changes.
    /// </summary>
    event EventHandler<StreamDeviceStatus> DeviceStatus;

    /// <summary>
    /// Occurs when an error occurs while receiving a message.
    /// </summary>
    event EventHandler<DeviceErrorEventArgs> MessageReceivedError;

    /// <summary>
    /// Occurs when an error occurs while transmitting a message.
    /// </summary>
    event EventHandler<DeviceErrorEventArgs> MessageTransmitterError;

    /// <summary>
    /// Transmits a message to the device asynchronously.
    /// </summary>
    /// <param name="message">The message to transmit.</param>
    /// <returns>A task representing the asynchronous operation, containing <c>true</c> if the transmission was successful; otherwise, <c>false</c>.</returns>
    new Task<bool> Transmit(TMessage message);
}