using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Communications.Contracts.Handler
{
    public interface IDeferralManager
    {
        Task<Guid> PostAsync(IDeferralRequest model);
        Task<IEnumerable<IDeferralWaiting>> GetWaitingRequestsAsync(DateTimeOffset checkTime, int maxCount);
        Task SendAsync(Guid notificationDeferralId, Guid outboundCorrelationId);
        Task ErrorAsync(Guid notificationDeferralId, Exception exception);
    }
}
