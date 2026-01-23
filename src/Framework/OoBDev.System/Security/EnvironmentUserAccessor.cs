using System;

namespace OoBDev.System.Security;

/// <summary>
/// Provides access to the current user's identity using environment variables.
/// </summary>
public class EnvironmentUserAccessor : ICurrentUserAccessor
{
    /// <summary>
    /// Gets the current user's name in the format "UserName@DomainName".
    /// </summary>
    public string? UserName => $"{Environment.UserName}@{Environment.UserDomainName}";
}
