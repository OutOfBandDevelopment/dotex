using OoBDev.MessageQueueing.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.SqlServer.Server.MessageQueueing;

/// <summary>
/// Provides message queue functionality using SQL Server Service Broker.
/// </summary>
/// <remarks>
/// This implementation is incomplete and will throw <see cref="NotImplementedException"/> for all operations.
/// </remarks>
public class SqlServiceBrokerQueueMessageProvider : IMessageSenderProvider, IMessageReceiverProvider
{
    //TODO: finish this out
    /// <inheritdoc/>
    public Task RunAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<string?> SendAsync(object message, IMessageContext context) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public IMessageReceiverProvider SetHandlerProvider(IMessageHandlerProvider handlerProvider) =>
        throw new NotImplementedException();
}
