using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Authorization;

/// <summary>Reads the durable right audit trail. Swappable: the default implementation is in <c>OoBDev.AspNetCore.EntityFrameworkCore</c>.</summary>
public interface IRightAuditQuery
{
    /// <summary>Reads the trail, newest first.</summary>
    /// <param name="filter">The filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching entries.</returns>
    Task<IReadOnlyList<RightAuditEntry>> QueryAsync(RightAuditFilter filter, CancellationToken cancellationToken = default);
}
