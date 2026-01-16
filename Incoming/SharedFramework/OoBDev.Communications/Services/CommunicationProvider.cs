using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Contracts.Models;
using OoBDev.Communications.Contracts.Services;
using OoBDev.MessageQueueing.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OoBDev.Communications.Services
{
    [MessageQueue(
        QueueName = SystemQueues.CommunicationCentral,
        QueueType = QueueTypes.AzureStorageQueue
        )]
    public class CommunicationProvider : ICommunicationProvider
    {
        private readonly IAttributeResolver _messageType;
        private readonly IMessageSender<CommunicationProvider> _queue;
        private readonly ILogger<CommunicationProvider> _log;

        public CommunicationProvider(
            IAttributeResolver messageType,
            IMessageSender<CommunicationProvider> queue,
            ILogger<CommunicationProvider> log
            )
        {
            _messageType = messageType;
            _queue = queue;
            _log = log;
        }

        //TODO: this version should be internal
        public Task<string> SendAsync(
            ISendRequest model,
            [CallerMemberName] string? caller = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string? callerPath = null
            )
        {
            if (model == null)
            {
                _log.LogError($"{nameof(model)} must exist");
                throw new ArgumentNullException(nameof(model));
            }
            if (model.TargetPersonId == Guid.Empty)
            {
                _log.LogError($"{nameof(model.TargetPersonId)} must not be empty");
                throw new ArgumentException($"{nameof(model.TargetPersonId)} must not be empty", nameof(model.TargetPersonId));
            }
            if (string.IsNullOrWhiteSpace(model.MessageType))
            {
                _log.LogError($"{nameof(model.MessageType)} must be provided");
                throw new ArgumentException($"{nameof(model.MessageType)} must be provided");
            }

            return _queue.SendAsync(
                model,
                caller: caller,
                lineNumber: lineNumber,
                callerPath: callerPath
            );
        }

        //TODO: this version should be removed
        public Task<string> SendAsync(
            Guid targetPersonId,
            string messageType,
            object? extendedData,
            [CallerMemberName] string? caller = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string? callerPath = null
            ) =>
            SendAsync(new SendRequestModel
            {
                TargetPersonId = targetPersonId,
                MessageType = messageType,
                Data = extendedData,
            },
            caller: caller,
            lineNumber: lineNumber,
            callerPath: callerPath
            );

        public Task<string> SendAsync<T>(
            Guid targetPersonId,
            T messageData,
            [CallerMemberName] string? caller = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string? callerPath = null
            ) =>
            SendAsync(new SendRequestModel
            {
                TargetPersonId = targetPersonId,
                MessageType = _messageType.GetMessageType<T>(),
                Data = messageData,
                Priority = _messageType.GetPriority<T>(),
            },
            caller: caller,
            lineNumber: lineNumber,
            callerPath: callerPath
            );
    }
}
