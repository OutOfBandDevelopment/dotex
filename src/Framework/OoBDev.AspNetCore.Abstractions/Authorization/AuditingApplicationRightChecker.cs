using Microsoft.Extensions.Logging;
using OoBDev.System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace OoBDev.AspNetCore.Authorization;

/// <summary>
/// Wraps an <see cref="IApplicationRightChecker"/> and writes one structured log entry per check: who asked,
/// which rights were required and the outcome. Denials log at warning, allowed checks at information.
/// An optional <see cref="IRightAuditSink"/> also receives each check for a durable trail.
/// </summary>
public partial class AuditingApplicationRightChecker(
    IApplicationRightChecker inner,
    ILogger<AuditingApplicationRightChecker> logger,
    IRightAuditSink? sink = null,
    TimeProvider? timeProvider = null) : IApplicationRightChecker
{
    /// <inheritdoc/>
    public RightCheckResult Check(ClaimsPrincipal principal, IReadOnlyCollection<string> requiredRights)
    {
        var result = inner.Check(principal, requiredRights);
        var subject = principal?.FindFirst(CommonClaims.NameIdentifier)?.Value ?? principal?.Identity?.Name ?? "anonymous";
        var rights = requiredRights == null ? string.Empty : string.Join(',', requiredRights);

        if (result == RightCheckResult.Allowed)
        {
            LogAllowed(logger, subject, rights);
        }
        else
        {
            LogDenied(logger, subject, rights, result);
        }

        sink?.Record(new RightAuditEntry((timeProvider ?? TimeProvider.System).GetUtcNow(), subject, rights, result.ToString()));
        return result;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Rights allowed: subject {Subject}, required [{Rights}]")]
    private static partial void LogAllowed(ILogger logger, string subject, string rights);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Rights denied ({Result}): subject {Subject}, required [{Rights}]")]
    private static partial void LogDenied(ILogger logger, string subject, string rights, RightCheckResult result);
}
