using OoBDev.Communications.Handler;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Communications.Contracts.Handler
{
    public interface ICommunicationCentralProcessor
    {
        DateTimeOffset? IsDeferredUntil(ITargetPreference preference, ISendRequest request, DateTimeOffset checkTime);
        Task DeferRequestAsync(ITargetPreference preference, Guid correlationId, ISendRequest request, DateTimeOffset until, IDictionary<string, object> headers);
        Task HandleRequestAsync(ITargetPreference preference, Guid correlationId, ISendRequest request, IDictionary<string, object> headers);
    }
}
