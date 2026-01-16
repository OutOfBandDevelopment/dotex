using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Communications.Handler
{

    public class CommunicationCentralProvider : ICommunicationCentralProvider
    {
        private readonly ITargetPreferenceManager _target;
        private readonly ICommunicationCentralProcessor _process;
        private readonly ILogger<CommunicationCentralProvider> _log;
        private readonly IDateTools _date;
        private readonly IGuidTools _guid;

        public CommunicationCentralProvider(
            ITargetPreferenceManager target,
            ICommunicationCentralProcessor process,
            ILogger<CommunicationCentralProvider> log,
            IDateTools date,
            IGuidTools guid
            )
        {
            _target = target;
            _process = process;
            _log = log;
            _date = date;
            _guid = guid;
        }

        public async Task<Guid> ReceivedAsync(ISendRequest received, Guid correlationId, IDictionary<string, object> headers)
        {
            try
            {
                var preference = await _target.GetTargetPreferencesAsync(received.TargetPersonId, received.MessageType).ConfigureAwait(false);
                var requestgroupId = correlationId == Guid.Empty ? _guid.NewGuid() : correlationId;
                _log.LogInformation($"Start {nameof(ReceivedAsync)} for {correlationId}/{requestgroupId}@{received.TargetPersonId}: {received.MessageType}");

                var deferredUntil = _process.IsDeferredUntil(preference, received, _date.UtcNow());
                if (received.Priority != RequestPriorities.Immediate && deferredUntil.HasValue)
                {
                    _log.LogInformation($"Defer {nameof(ReceivedAsync)} for {correlationId}/{requestgroupId}@{received.TargetPersonId}: {received.MessageType} until: {deferredUntil}");
                    await _process.DeferRequestAsync(preference, requestgroupId, received, deferredUntil.Value, headers).ConfigureAwait(false);
                }
                else
                {
                    _log.LogInformation($"Handle {nameof(ReceivedAsync)} for {correlationId}/{requestgroupId}@{received.TargetPersonId}: {received.MessageType}");
                    await _process.HandleRequestAsync(preference, requestgroupId, received, headers).ConfigureAwait(false);
                }
                _log.LogInformation($"Completed {nameof(ReceivedAsync)} for {correlationId}/{requestgroupId}@{received.TargetPersonId}: {received.MessageType}");

                return requestgroupId;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed: {nameof(ReceivedAsync)} for {correlationId}@{received.TargetPersonId}: {received.MessageType}: {ex.Message}");
                _log.LogDebug($"Failed: {nameof(ReceivedAsync)} for {correlationId}@{received.TargetPersonId}: {received.MessageType}: {ex}");
                throw;
            }
        }
    }
}
