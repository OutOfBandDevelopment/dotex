using System;

namespace OoBDev.AspNetCore.AuthGateway;

/// <summary>The token endpoint refused or did not answer; the message never carries a secret or a token.</summary>
public sealed class TokenRequestException : Exception
{
    public TokenRequestException(string message) : base(message)
    {
    }

    public TokenRequestException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
