using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Services;
using OoBDev.MessageQueueing.Contracts;
using OoBDev.Toolkit.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Common.Services
{
    [MessageQueue(
        QueueName = SystemQueues.ComplexEvents,
        QueueType = QueueTypes.AzureStorageQueue,
        Priority = int.MaxValue - 1
        )]
    //TODO:Add the ability to configure queue type at runtime
    //[MessageQueue(
    //    QueueName = SystemQueues.ComplexEventQueue,
    //    QueueType = QueueTypes.Default,
    //    Priority = int.MaxValue
    //    )]
    [ServiceConfig(
        Priority = int.MaxValue
        )]
    public class EnqueueEventHubProvider<TChannel> : IEventHubProvider<TChannel>
    {
        private readonly IEventResolver _resolver;
        private readonly IMessageSender<EnqueueEventHubProvider<TChannel>> _queue;

        public EnqueueEventHubProvider(
            IEventResolver resolver,
            IMessageSender<EnqueueEventHubProvider<TChannel>> queue
            )
        {
            _resolver = resolver;
            _queue = queue;
        }

        public async Task SendAsync<TEvent>(
            TEvent item,
            IDictionary<string, object> properties
            ) where TEvent : IEventData
        {
            var messageType = _resolver.GetMessageType(item);
            var evnt = new ComplexEventData
            {
                ClassType = messageType,
                Data = item,
            };

            foreach (var property in properties)
            {
                evnt.Properties.Add(property.Key, property.Value);
            }

            object? callerValue = null;
            object? lineNumberValue = null;
            object? callerPathValue = null;
            properties?.TryGetValue("X-CallerMemberName", out callerValue);
            properties?.TryGetValue("X-CallerLineNumber", out lineNumberValue);
            properties?.TryGetValue("X-CallerFilePath", out callerPathValue);

            string? caller = null;
            int? lineNumber = null;
            string? callerPath = null;

            if (callerValue is string c) caller = c;
            if (lineNumberValue is int l) lineNumber = l;
            if (callerPathValue is string cp) callerPath = cp;

            await _queue.SendAsync(
                message: evnt,
                messageId: default,
                caller: caller ?? default,
                lineNumber: lineNumber ?? default,
                callerPath: callerPath ?? default
                ).ConfigureAwait(false);
        }
    }
}
