using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Channels;
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
    [Composer(DeliveryChannel = None)]
    public class NoMessageComposer : IMessageComposer
    {
        private readonly IDeliveryPersitenceProvider _delivery;
        private readonly ILogger<NoMessageComposer> _log;
        private readonly IDateTools _date;

        public NoMessageComposer(
            IDeliveryPersitenceProvider delivery,
            ILogger<NoMessageComposer> log,
            IDateTools date
            )
        {
            _delivery = delivery;
            _log = log;
            _date = date;
        }

        public async Task ComposeAndSendAsync(Guid targetPersonId, string messageType, CultureInfo? culture, JObject data, Guid requestGroupId, IDictionary<string, object> headers)
        {
            _log.LogDebug($@"Target: {targetPersonId} for ""{messageType}""/{culture} ({requestGroupId})");

            var requestId = await _delivery.RequestedAsync(new CreateDeliveryRequestModel
            {
                PersonId = targetPersonId,
                Requested = _date.Now(),
                RequestGroupId = requestGroupId,

                DeliveryChannel = DeliveryChannelType.None,
                MessageType = messageType,

                SentTo = None,
                Subject = null,
                Text = null,
                Html = null,

                EnhancedData = data.ToString(),
            });
            await _delivery.SuccessAsync(requestId, _date.Now(), "Not Sent");
        }
    }
}