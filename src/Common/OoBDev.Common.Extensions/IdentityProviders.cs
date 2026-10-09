using System;

namespace OoBDev.Common.Extensions;

/// <summary>
/// Specifies different identity providers for authentication.
/// </summary>
[Flags]
public enum IdentityProviders
{
    /// <summary>
    /// Represents no specific identity provider.
    /// </summary>
    None = 0,

    /// <summary>
    /// Represents the Keycloak identity provider.
    /// </summary>
    Keycloak = 0b10,
}
