using OoBDev.MessageQueueing.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OoBDev.Azure.ServiceBus.Tests.MessageQueueing
{
    public class TestSource
    {
        private readonly IMessageSender<TestQueueTarget> _queue;
        private readonly IMessageSender<TestTopicTarget> _topic;

        public TestSource(
            IMessageSender<TestQueueTarget> queue,
            IMessageSender<TestTopicTarget> topic
            )
        {
            _queue = queue;
            _topic = topic;
        }

        public async Task<string> SendQueueAsync<T>(
            T message,
            string messageId = null,
            [CallerMemberName] string caller = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string callerPath = null
            ) where T : class => await _queue.SendAsync(message, messageId, caller, lineNumber, callerPath);

        public async Task<string> SendTopicAsync<T>(
            T message,
            string messageId = null,
            [CallerMemberName] string caller = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string callerPath = null
            ) where T : class => await _topic.SendAsync(message, messageId, caller, lineNumber, callerPath);

    }
}
