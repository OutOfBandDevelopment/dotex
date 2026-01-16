using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Contracts.Models;
using OoBDev.MessageQueueing.Contracts;
using OoBDev.Toolkit.Contracts.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Provider
{
    public class SendSmsProvider : ISendSmsProvider
    {
        private readonly IMessageSender<SmsMessageModel> _queue;
        private readonly ISelectedService<ISendSmsHandler> _handler;
        private readonly ILogger<SendSmsProvider> _log;


        public SendSmsProvider(
            IMessageSender<SmsMessageModel> queue,
            ISelectedService<ISendSmsHandler> handler,
            ILogger<SendSmsProvider> log
            )
        {
            _queue = queue;
            _handler = handler;
            _log = log;
        }

        public async Task<string?> ScheduleSendMessageAsync(ISmsMessage message, string? messageId = null) =>
            await _queue.SendAsync(message, messageId);

        public async Task SendMessageAsync(ISmsMessage message)
        {
            try
            {
                _log.LogInformation($"{nameof(SendSmsProvider)}:{nameof(SendMessageAsync)}:{message.RequestId}");
                await _handler.Value.SendMessageAsync(message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                _log.LogDebug(ex.ToString());
                throw;
            }
        }
    }
}
