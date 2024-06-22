namespace OoBDev.System.IO;

/// <summary>
/// Represents a basic device interface.
/// </summary>
public interface IDevice
{
}
/// <summary>
/// Represents a device interface that handles messages of type <typeparamref name="TMessage"/>.
/// </summary>
/// <typeparam name="TMessage">The type of messages handled by the device.</typeparam>
public interface IDevice<TMessage> : IDevice
{
}
