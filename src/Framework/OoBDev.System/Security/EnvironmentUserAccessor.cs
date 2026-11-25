using System;
using System.Runtime.Versioning;
using System.Security.Claims;
using System.Security.Principal;

namespace OoBDev.System.Security;

/// <summary>
/// Provides the current user from the operating system session: the Windows identity on Windows, otherwise the
/// environment user name. Intended for command line and desktop applications.
/// </summary>
public class EnvironmentUserAccessor : ICurrentUserAccessor
{
    private readonly Lazy<ClaimsPrincipal> _user = new(Create);

    /// <summary>
    /// Gets the principal of the active operating system session.
    /// </summary>
    public ClaimsPrincipal? User => _user.Value;

    /// <summary>
    /// Gets the current user's name in the format "UserName@DomainName".
    /// </summary>
    public string? UserName => $"{Environment.UserName}@{Environment.UserDomainName}";

    private static ClaimsPrincipal Create()
    {
        if (OperatingSystem.IsWindows())
            return FromWindows();

        return new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, $"{Environment.UserName}@{Environment.UserDomainName}")],
            authenticationType: "Environment"));
    }

    [SupportedOSPlatform("windows")]
    private static ClaimsPrincipal FromWindows() => new(WindowsIdentity.GetCurrent());
}
