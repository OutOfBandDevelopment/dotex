using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications.Orchestration;

/// <summary>Limits an enhancer to the message type named on its <see cref="CommunicationAttribute"/>.</summary>
/// <param name="inner">The discovered enhancer.</param>
/// <param name="messageType">The message type from the attribute.</param>
internal sealed class MessageTypeEnhancer(IDataEnhancer inner, string messageType) : IDataEnhancer
{
    public bool AppliesTo(string type) => string.Equals(type, messageType, StringComparison.OrdinalIgnoreCase) && inner.AppliesTo(type);

    public Task EnhanceAsync(CommunicationRequest request, IDictionary<string, object?> data, CancellationToken cancellationToken = default)
        => inner.EnhanceAsync(request, data, cancellationToken);
}
