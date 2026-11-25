using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Adds context (names, order lines, links) to the data of a request before it is composed.</summary>
public interface IDataEnhancer
{
    /// <summary>Gets whether this enhancer applies to <paramref name="messageType"/>.</summary>
    bool AppliesTo(string messageType);

    /// <summary>Adds or changes values in <paramref name="data"/>, in registration order.</summary>
    Task EnhanceAsync(CommunicationRequest request, IDictionary<string, object?> data, CancellationToken cancellationToken = default);
}
