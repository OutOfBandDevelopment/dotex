using System.Collections.Generic;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Factory for creating instances of <see cref="IMessageReceiverProvider"/>.
/// </summary>
public interface IMessageReceiverProviderFactory
{
    /// <summary>
    /// Creates a collection of message receiver providers.
    /// </summary>
    /// <returns>An enumerable collection of message receiver providers.</returns>
    IEnumerable<IMessageReceiverProvider> Create();
}
