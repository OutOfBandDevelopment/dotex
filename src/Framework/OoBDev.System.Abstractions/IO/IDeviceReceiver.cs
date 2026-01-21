using System;

namespace OoBDev.System.IO;

/// <summary>
/// Represents a device that can receive messages.
/// </summary>
/// <typeparam name="TMessage">The type of messages the device can receive.</typeparam>
public interface IDeviceReceiver<TMessage> : IDevice<TMessage>
{
    /// <summary>
    /// Occurs when a message is received from the device.
    /// </summary>
    event EventHandler<TMessage> MessageReceived;
}
