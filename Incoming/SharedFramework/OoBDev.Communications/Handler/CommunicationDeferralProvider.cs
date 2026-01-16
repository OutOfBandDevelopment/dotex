using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Contracts.Models;
using OoBDev.Toolkit.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Communications.Handler
{
    public class CommunicationDeferralProvider : ICommunicationDeferralProvider
    {
        private readonly IObjectDeserializer _deserializer;
        private readonly ICommunicationProvider _provider;
        private readonly IDeferralManager _manager;
        private readonly ILogger<CommunicationDeferralProvider> _log;
        private readonly ICommunicationDeferralConfig _config;

        public CommunicationDeferralProvider(
            IObjectDeserializer deserializer,
            ICommunicationProvider provider,
            IDeferralManager manager,
            ILogger<CommunicationDeferralProvider> log,
            ICommunicationDeferralConfig config
            )
        {
            _deserializer = deserializer;
            _provider = provider;
            _manager = manager;
            _log = log;

            _config = config;
        }

        public async Task<bool> ExecuteAsync(DateTimeOffset checkTime)
        {
            var waiting = (await _manager.GetWaitingRequestsAsync(checkTime, _config.MaxCount)).ToArray();
            if (waiting.Any())
            {
                _log.LogInformation($"Processing {waiting.Length} messages");

                foreach (var deferral in waiting)
                {
                    try
                    {
                        //TODO: this should be made into an internal method
                        var response = await _provider.SendAsync(new SendRequestModel
                        {
                            TargetPersonId = deferral.TargetPersonId,
                            MessageType = deferral.MessageType,
                            Data = _deserializer.Deserialize(deferral.ExtendedData),
                            Priority = RequestPriorities.Immediate,
                        }).ConfigureAwait(false);

                        Guid.TryParse(response, out var correlationId);
                        await _manager.SendAsync(deferral.NotificationDeferralId, correlationId).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await _manager.ErrorAsync(deferral.NotificationDeferralId, ex).ConfigureAwait(false);
                        _log.LogError($"{nameof(deferral.NotificationDeferralId)}: {deferral.NotificationDeferralId}- {ex.Message}");
                        _log.LogDebug($"{nameof(deferral.NotificationDeferralId)}: {deferral.NotificationDeferralId}- {ex}");
                        //we don't want a failed request possibly re-queue a successful request so just log and dequeue
                        throw;
                    }
                }
                _log.LogInformation($"Processed {waiting.Length} messages");
                return true;
            }
            else
            {
                _log.LogInformation($"No pending messages");
                return true;
            }
        }

    }
}
