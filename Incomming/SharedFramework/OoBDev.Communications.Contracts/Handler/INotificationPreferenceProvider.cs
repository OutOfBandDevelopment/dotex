using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Contracts.Handler
{
    public interface INotificationPreferenceProvider
    {
        Task<IDeliveryPreference> GetDeliveryPreferencesAsync(Guid personId, string messageType);
    }
}
