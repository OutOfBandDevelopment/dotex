using OoBDev.MessageQueueing.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.SqlServer.Server.MessageQueueing;

public class SqlServiceBrokerQueueMessageProvider : IMessageSenderProvider, IMessageReceiverProvider
{
    //TODO: finish this out
    public Task RunAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
    public Task<string?> SendAsync(object message, IMessageContext context) =>
        throw new NotImplementedException();
    public IMessageReceiverProvider SetHandlerProvider(IMessageHandlerProvider handlerProvider) =>
        throw new NotImplementedException();
}
