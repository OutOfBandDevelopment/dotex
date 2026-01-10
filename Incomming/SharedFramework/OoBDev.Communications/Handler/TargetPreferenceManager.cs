using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Contracts.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Communications.Handler
{
    public class TargetPreferenceManager : ITargetPreferenceManager
    {
        private readonly INotificationPreferenceProvider _provider;

        public TargetPreferenceManager(
            INotificationPreferenceProvider provider
            )
        {
            _provider = provider;
        }

        public async Task<ITargetPreference> GetTargetPreferencesAsync(Guid targetPersonId, string messageType)
        {
            var preference = await _provider.GetDeliveryPreferencesAsync(targetPersonId, messageType).ConfigureAwait(false);

            return new TargetPreferenceModel
            {
                Channels = preference?.Channels?.Any() ?? false ? preference.Channels : new[] { DeliveryChannels.None, },
                Culture = preference?.Culture,

                SkipWeekends = preference?.SkipWeekends ?? false,
                StartTime = preference?.StartTime,
                EndTime = preference?.EndTime,
                TimeZone = preference?.TimeZone,
            };
        }
    }
}