using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions.Handler
{
    public interface ICommunicationCentralProvider
    {
        Task<Guid> ReceivedAsync(ISendRequest received, Guid correlationId, IDictionary<string, object> headers);
    }
}
