using OoBDev.Communications.Handler;
using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions.Handler
{
    public interface ITargetPreferenceManager
    {
        Task<ITargetPreference> GetTargetPreferencesAsync(Guid targetPersonId, string messageType);
    }
}