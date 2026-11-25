namespace OoBDev.AspNetCore.Mvc.Diagnostics.HealthChecks;

/// <summary>
/// How much of a health report a caller sees.
/// </summary>
public enum HealthCheckDetailLevel
{
    /// <summary>The overall status only (anonymous callers).</summary>
    Status,

    /// <summary>The overall status and each check's name and status (authenticated callers).</summary>
    Checks,

    /// <summary>Everything: descriptions, data and errors (callers holding the detail right).</summary>
    Details,
}
