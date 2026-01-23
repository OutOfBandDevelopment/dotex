using System.Threading.Tasks;

namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Represents an asynchronous callback that is invoked when a message is received.
/// </summary>
/// <param name="message">The received message object.</param>
/// <returns>A task representing the asynchronous message handling operation.</returns>
public delegate Task OnMessageReceived(object message);

/// <summary>
/// Represents a strongly-typed asynchronous callback that is invoked when a message is received.
/// </summary>
/// <typeparam name="TMessage">The type of the message.</typeparam>
/// <param name="message">The received message.</param>
/// <returns>A task representing the asynchronous message handling operation.</returns>
public delegate Task OnMessageReceived<TMessage>(TMessage message);
