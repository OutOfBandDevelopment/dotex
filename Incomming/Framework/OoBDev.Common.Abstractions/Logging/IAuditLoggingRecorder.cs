using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace OoBDev.Common.Logging;

public interface IAuditLoggingRecorder
{
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
    IAsyncEnumerable<AuditLogResponse> RecordAsync(AuditLogEntry entry, [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
}
