namespace OoBDev.AspNetCore.Authorization;

/// <summary>
/// Outcome of an application right check.
/// </summary>
public enum RightCheckResult
{
    /// <summary>The caller may proceed.</summary>
    Allowed,

    /// <summary>The caller is not authenticated (401).</summary>
    Unauthenticated,

    /// <summary>The caller is authenticated but holds none of the required rights (403).</summary>
    Forbidden,
}
