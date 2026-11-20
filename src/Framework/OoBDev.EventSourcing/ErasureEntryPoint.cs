using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>The default <see cref="IErasureEntryPoint"/>: appends the tombstone through <see cref="ErasureService"/> under a new request id.</summary>
public sealed class ErasureEntryPoint(ErasureService service) : IErasureEntryPoint
{
    /// <inheritdoc />
    public async Task<string> RequestAsync(string subjectId, IReadOnlyList<string>? knownCopies = null, CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        await service.RequestAsync(subjectId, requestId, knownCopies, cancellationToken).ConfigureAwait(false);
        return requestId;
    }
}
