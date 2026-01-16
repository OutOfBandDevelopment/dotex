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
    public class SendEmailProvider : ISendEmailProvider
    {
        private readonly IMessageSender<EmailMessageModel> _queue;
        private readonly ISelectedService<ISendEmailHandler> _handler;
        private readonly ILogger<SendEmailProvider> _log;

        public SendEmailProvider(
            IMessageSender<EmailMessageModel> queue,
            ISelectedService<ISendEmailHandler> handler,
            ILogger<SendEmailProvider> log
            )
        {
            _queue = queue;
            _handler = handler;
            _log = log;
        }

        public async Task<string?> ScheduleSendMessageAsync(IEmailMessage message, string? messageId = null) =>
            await _queue.SendAsync(message, messageId);

        public async Task SendMessageAsync(IEmailMessage message)
        {
            try
            {
                _log.LogInformation($"{nameof(SendEmailProvider)}:{nameof(SendMessageAsync)}:{message.RequestId}");
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
