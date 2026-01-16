using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.Composers;
using OoBDev.Communications.Contracts.DeliveryLog;
using OoBDev.Communications.Contracts.Models;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using static OoBDev.Communications.Contracts.DeliveryChannels;

namespace OoBDev.Communications.Composers
{
    [Composer(DeliveryChannel = Sms)]
    public class SmsMessageComposer : IMessageComposer
    {
        private readonly ITemplateProvider _template;
        private readonly IPersonContactProvider _person;
        private readonly ISendSmsProvider _sms;
        private readonly IDeliveryPersitenceProvider _delivery;
        private readonly ILogger<SmsMessageComposer> _log;
        private readonly IDateTools _date;

        public SmsMessageComposer(
            ITemplateProvider template,
            IPersonContactProvider person,
            ISendSmsProvider sms,
            IDeliveryPersitenceProvider delivery,
            ILogger<SmsMessageComposer> log,
            IDateTools date
            )
        {
            _template = template;
            _person = person;
            _sms = sms;
            _delivery = delivery;
            _log = log;
            _date = date;
        }

        public async Task ComposeAndSendAsync(Guid targetPersonId, string messageType, CultureInfo? culture, JObject data, Guid requestGroupId, IDictionary<string, object> headers)
        {
            //TODO: we need model validation
            _log.LogDebug($@"Target: {targetPersonId} for ""{messageType}""/{culture} ({requestGroupId})");
            var model = new SmsMessageModel
            {
                MessageType = messageType,
                Body = await _template.GetTemplateAsync(messageType, Sms, "Body", culture, data).ConfigureAwait(false),
                To = await _person.GetSmsAsync(personId: targetPersonId).ConfigureAwait(false),
                From = await _template.GetTemplateAsync(messageType, Sms, "From", culture, data).ConfigureAwait(false),

                Headers = headers ?? new Dictionary<string, object>(),
            };
            model.RequestId = await _delivery.RequestedAsync(new CreateDeliveryRequestModel
            {
                PersonId = targetPersonId,
                Requested = _date.Now(),
                RequestGroupId = requestGroupId,

                DeliveryChannel = DeliveryChannelType.Sms,
                MessageType = messageType,

                SentTo = model.To,
                Subject = null,
                Text = model.Body,
                Html = null,

                EnhancedData = data.ToString(),
            });
            _log.LogInformation($@"Target: {targetPersonId} for ""{messageType}""/{culture} ({requestGroupId}) as {model.RequestId}");
            try
            {
                await _sms.ScheduleSendMessageAsync(model).ConfigureAwait(false);
                _log.LogDebug($@"Target: {targetPersonId} for ""{messageType}""/{culture} ({requestGroupId}) as {model.RequestId} Scheduled");
            }
            catch (Exception ex)
            {
                _log.LogError($@"Target: {targetPersonId} for ""{messageType}""/{culture} ({requestGroupId}) as {model.RequestId} !! {ex.Message}");
                _log.LogDebug($@"Target: {targetPersonId} for ""{messageType}""/{culture} ({requestGroupId}) as {model.RequestId} !! {ex}");
                if (!await _delivery.FailedAsync(model.RequestId, _date.Now(), ex))
                    throw;
            }
        }
    }
}