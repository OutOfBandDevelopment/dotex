using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Contracts.Models;
using OoBDev.Toolkit.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OoBDev.Communications.Contracts.RequestPriorities;
using static System.DayOfWeek;

namespace OoBDev.Communications.Handler
{
    public class CommunicationCentralProcessor : ICommunicationCentralProcessor
    {
        private readonly IDataEnhancementManager _data;
        private readonly IMessageComposerFactory _message;
        private readonly ILogger<CommunicationCentralProcessor> _log;
        private readonly IDeferralManager _deferral;
        private readonly IObjectSerializer _serializer;

        public CommunicationCentralProcessor(
            IDataEnhancementManager data,
            IMessageComposerFactory message,
            ILogger<CommunicationCentralProcessor> log,
            IDeferralManager deferral,
            IObjectSerializer serializer
            )
        {
            _data = data;
            _message = message;
            _log = log;
            _deferral = deferral;
            _serializer = serializer;
        }

        private JObject SeedData(Guid correlationId, ISendRequest request, IDictionary<string, object> headers) =>
                    _data.SeedData(request.Data,
                        ("Request-Headers", headers),
                        ("Request-MessageType", request.MessageType),
                        ("Request-CorrelationId", correlationId)
                        );

        public async Task DeferRequestAsync(ITargetPreference preference, Guid correlationId, ISendRequest request, DateTimeOffset until, IDictionary<string, object> headers) =>
           await _deferral.PostAsync(new DeferralRequestModel
           {
               CorrelationId = correlationId,
               TargetPersonId = request.TargetPersonId,
               MessageType = request.MessageType,
               ExtendedData = _serializer.GetAsSerialized(SeedData(correlationId, request, headers)),
               HoldUntil = until,
           }).ConfigureAwait(false);

        public async Task HandleRequestAsync(ITargetPreference preference, Guid correlationId, ISendRequest request, IDictionary<string, object> headers)
        {
            _log.LogInformation($"{nameof(HandleRequestAsync)}: {correlationId}/{request.MessageType} to {request.TargetPersonId}");
            if (preference?.Channels?.Any() ?? false)
            {
                //TODO: this should be caught and directed somewhere so validation fails can be sent somewhere outside of the flow
                var data = await _data.EnhanceAsync(
                    request.TargetPersonId,
                    request.MessageType,
                    SeedData(correlationId, request, headers)
                    ).ConfigureAwait(false);
                _log.LogInformation($"{nameof(HandleRequestAsync)}-Enhanced: {correlationId}/{request.MessageType} to {request.TargetPersonId}");

                await ProcessChannelsAsync(preference.Channels, preference, correlationId, request, data, headers).ConfigureAwait(false);
                _log.LogInformation($"{nameof(HandleRequestAsync)}-Sent: {correlationId}/{request.MessageType} to {request.TargetPersonId}");
            }
            else
            {
                _log.LogWarning($"{nameof(HandleRequestAsync)}: {correlationId}/{request.MessageType} to {request.TargetPersonId}.  No Channels Provided");
            }
        }

        private Task ProcessChannelsAsync(string[] channels, ITargetPreference preference, Guid correlationId, ISendRequest request, JObject data, IDictionary<string, object> headers) =>
            Task.WhenAll(channels.Select(channel => BuildAndProcessChannelAsync(channel, preference, correlationId, request, data, headers)));

        private async Task BuildAndProcessChannelAsync(string channel, ITargetPreference preference, Guid correlationId, ISendRequest request, JObject data, IDictionary<string, object> headers)
        {
            _log.LogInformation($"{nameof(BuildAndProcessChannelAsync)}({channel}): {correlationId}/{request.MessageType} to {request.TargetPersonId}");

            try
            {
                var composer = _message.GetComposer(channel);
                if (composer == null)
                {
                    _log.LogInformation($"{nameof(BuildAndProcessChannelAsync)}({channel}): {correlationId}/{request.MessageType} to {request.TargetPersonId} -- No Composer Found");
                    return;
                }

                _log.LogInformation($"{nameof(BuildAndProcessChannelAsync)}({channel}): {correlationId}/{request.MessageType} to {request.TargetPersonId} -- {composer}");

                await composer.ComposeAndSendAsync(
                                request.TargetPersonId,
                                request.MessageType,
                                preference.Culture,
                                data,
                                correlationId,
                                headers
                            );

                _log.LogInformation($"{nameof(BuildAndProcessChannelAsync)}({channel}): {correlationId}/{request.MessageType} to {request.TargetPersonId} -- Send!");
            }
            catch (Exception ex)
            {
                _log.LogError($"{nameof(BuildAndProcessChannelAsync)}({channel}):{correlationId}/{request.MessageType} to {request.TargetPersonId} !!ERROR: {ex.Message}");
                _log.LogDebug($"{ex}");
                throw;
                //if an exception occurs the event should have been logged as sent and we don't want to resent those that were successful
            }
        }

        public DateTimeOffset? IsDeferredUntil(ITargetPreference preference, ISendRequest request, DateTimeOffset checkTime)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.Priority == Immediate || preference == null)
            {
                _log.LogDebug($"{nameof(IsDeferredUntil)}: {request.MessageType} to {request.TargetPersonId} send Immediate");
                return null;
            }

            if (preference.StartTime.HasValue && preference.EndTime.HasValue)
            {
                _log.LogDebug($"{nameof(IsDeferredUntil)}: {request.MessageType} to {request.TargetPersonId} check times");

                var local = preference.TimeZone.HasValue ?
                            checkTime.ToOffset(preference.TimeZone.Value) :
                            checkTime;

                var start = new DateTimeOffset(local.Date.Add(preference.StartTime.Value), local.Offset);
                var end = new DateTimeOffset(local.Date.Add(preference.EndTime.Value).AddDays(preference.EndTime.Value < preference.StartTime.Value ? 1 : 0), local.Offset);

                if (preference.SkipWeekends)
                {
                    _log.LogDebug($"{nameof(IsDeferredUntil)}: {request.MessageType} to {request.TargetPersonId} check weekend");
                    var adjustment = local.DayOfWeek switch
                    {
                        Friday when local > end => 3,//After end of day Friday
                        Saturday => 2, //any time Saturday
                        Sunday => 1, //any time Sunday
                        _ => 0,
                    };

                    _log.LogDebug($"{nameof(IsDeferredUntil)}: {request.MessageType} to {request.TargetPersonId} adjust weekend by {adjustment}");
                    start = start.AddDays(adjustment);
                    end = end.AddDays(adjustment);
                }

                if (local < start)
                {
                    _log.LogDebug($"{nameof(IsDeferredUntil)}: {request.MessageType} to {request.TargetPersonId} requested time before range.. hold until {start}");
                    return start;
                }
                else if (local > end)
                {
                    start = start.AddDays(1);
                    _log.LogDebug($"{nameof(IsDeferredUntil)}: {request.MessageType} to {request.TargetPersonId} requested time after range.. hold until {start}");
                    return start;
                }
                else
                {
                    _log.LogDebug($"{nameof(IsDeferredUntil)}: {request.MessageType} to {request.TargetPersonId} requested time within range");
                }
            }
            else
            {
                _log.LogDebug($"{nameof(IsDeferredUntil)}: {request.MessageType} to {request.TargetPersonId} time range not configured");
            }

            return null;
        }
    }
}
