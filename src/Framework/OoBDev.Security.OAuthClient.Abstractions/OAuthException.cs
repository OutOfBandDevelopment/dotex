using System;

namespace OoBDev.Security.OAuthClient;

/// <summary>The provider refused a request, or the sign-in could not finish.</summary>
public sealed class OAuthException : Exception
{
    /// <summary>Creates the exception.</summary>
    /// <param name="error">The OAuth <c>error</c> code (for example <c>invalid_grant</c>).</param>
    /// <param name="description">The provider's <c>error_description</c>, when sent.</param>
    public OAuthException(string error, string? description = null)
        : base(string.IsNullOrEmpty(description) ? error : $"{error}: {description}")
    {
        Error = error;
        Description = description;
    }

    /// <summary>The OAuth <c>error</c> code.</summary>
    public string Error { get; }

    /// <summary>The provider's description, when sent.</summary>
    public string? Description { get; }
}
