using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions.Handler
{
    public interface INotificationPreferenceProvider
    {
        Task<IDeliveryPreference> GetDeliveryPreferencesAsync(Guid personId, string messageType);
    }
}
