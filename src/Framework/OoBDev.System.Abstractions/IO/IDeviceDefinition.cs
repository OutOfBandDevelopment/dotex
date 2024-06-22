namespace OoBDev.System.IO;

/// <summary>
/// Represents a generic device definition interface.
/// </summary>
public interface IDeviceDefinition
{
}
/// <summary>
/// Represents a device definition interface that handles messages of type <typeparamref name="TMessage"/>.
/// </summary>
/// <typeparam name="TMessage">The type of messages handled by the device.</typeparam>
public interface IDeviceDefinition<TMessage> : IDeviceDefinition
{
}
