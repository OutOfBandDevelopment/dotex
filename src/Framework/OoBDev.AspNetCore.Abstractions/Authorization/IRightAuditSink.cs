namespace OoBDev.AspNetCore.Authorization;

/// <summary>
/// Receives every right check without blocking the request. An implementation buffers and writes later (see
/// <c>OoBDev.AspNetCore.EntityFrameworkCore</c>); it must not throw.
/// </summary>
public interface IRightAuditSink
{
    /// <summary>Records one check.</summary>
    /// <param name="entry">The check.</param>
    void Record(RightAuditEntry entry);
}
