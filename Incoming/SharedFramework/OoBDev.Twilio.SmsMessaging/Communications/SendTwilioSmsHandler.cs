using OoBDev.Twilio.SmsMessaging.Shared;
using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.DeliveryLog;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Toolkit.Contracts.Common;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace OoBDev.Twilio.SmsMessaging.Communications
{
    [ServiceConfig(Priority = 10)]
    public class SendTwilioSmsHandler : ISendSmsHandler
    {
        private readonly ISmsClient _client;
        private readonly ILogger<SendTwilioSmsHandler> _log;
        private readonly IDeliveryPersitenceProvider _delivery;
        private readonly IDateTools _date;

        public SendTwilioSmsHandler(
            ISmsClient client,
            ILogger<SendTwilioSmsHandler> log,
            IDeliveryPersitenceProvider delivery,
            IDateTools date
            )
        {
            _client = client;
            _log = log;
            _delivery = delivery;
            _date = date;
        }

        public async Task SendMessageAsync(ISmsMessage message)
        {
            try
            {
                var response = await _client.SendMessageAsync(message).ConfigureAwait(false);
                await _delivery.SuccessAsync(message.RequestId, _date.Now(), response).ConfigureAwait(false);
                _log.LogDebug($@"Sent sms for ""{message.To}"" => ""{message.Body}""");
            }
            catch (Exception ex)
            {
                _log.LogError($@"{nameof(SendMessageAsync)}: ""{ex.Message}""");
                _log.LogDebug($@"{nameof(SendMessageAsync)}: ""{ex}""");

                if (!await _delivery.FailedAsync(message.RequestId, _date.Now(), ex).ConfigureAwait(false))
                    throw;
            }
        }
    }
}
