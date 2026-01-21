using OoBDev.Twilio.SendGrid.Shared;
using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.DeliveryLog;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace OoBDev.Twilio.SendGrid.Communications
{
    [ServiceConfig(Priority = 10)]
    public class SendGridEmailHandler : ISendEmailHandler
    {
        private readonly IEmailMessageMapper _mapper;
        private readonly IEmailClient _client;
        private readonly ILogger<SendGridEmailHandler> _log;
        private readonly IDeliveryPersitenceProvider _delivery;

        public SendGridEmailHandler(
            IEmailMessageMapper mapper,
            IEmailClient client,
            ILogger<SendGridEmailHandler> log,
            IDeliveryPersitenceProvider delivery
            )
        {
            _mapper = mapper;
            _client = client;
            _log = log;
            _delivery = delivery;
        }

        public async Task SendMessageAsync(IEmailMessage message)
        {
            try
            {
                var mesg = await _mapper.GetMessageAsync(message);
                var response = await _client.SendMessageAsync(message: mesg);

                await _delivery.SuccessAsync(message.RequestId, DateTimeOffset.Now, response);

                _log.LogInformation($@"Sent email for ""{string.Join(';', message.ToAddresses)}"" => ""{message.Subject}""");
            }
            catch (Exception ex)
            {
                _log.LogError($@"{nameof(SendMessageAsync)}: ""{ex.Message}""");
                _log.LogDebug($@"{nameof(SendMessageAsync)}: ""{ex}""");

                if (!await _delivery.FailedAsync(message.RequestId, DateTimeOffset.Now, ex))
                    throw;
            }
        }
    }
}
