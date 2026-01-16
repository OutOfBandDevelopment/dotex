using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Communications.Contracts.Handler
{
    public interface ICommunicationCentralProvider
    {
        Task<Guid> ReceivedAsync(ISendRequest received, Guid correlationId, IDictionary<string, object> headers);
    }
}
