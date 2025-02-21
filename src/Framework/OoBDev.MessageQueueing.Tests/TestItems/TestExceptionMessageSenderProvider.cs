using OoBDev.MessageQueueing.Services;
using System;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Tests.TestItems;

public class TestExceptionMessageSenderProvider : IMessageSenderProvider
{
    public Task<string?> SendAsync(object message, IMessageContext context) =>
        Task.FromException<string?>(new ApplicationException());
}
