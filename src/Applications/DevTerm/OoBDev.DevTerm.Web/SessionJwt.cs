using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace OoBDev.DevTerm.Web;

/// <summary>
/// Issues and validates the short-lived JWT a remote client gets in exchange for the shared key (<c>POST /api/token</c>).
/// Tokens are HS256-signed with a key that exists only in this process, so they stop working when the host restarts.
/// </summary>
public sealed class SessionJwt(TimeProvider? time = null, TimeSpan? lifetime = null)
{
    public const string Issuer = "dev-term-web";
    public const string ReadOnlyClaim = "devterm_readonly";

    private readonly TimeProvider clock = time ?? TimeProvider.System;
    private readonly SymmetricSecurityKey key = new(RandomNumberGenerator.GetBytes(32));
    private readonly JsonWebTokenHandler handler = new();

    /// <summary>How long a token lives.</summary>
    public TimeSpan Lifetime { get; } = lifetime ?? TimeSpan.FromMinutes(15);

    /// <summary>Creates a token; <paramref name="readOnly"/> marks a viewer that may watch but not send.</summary>
    public (string Token, DateTimeOffset Expires) Issue(bool readOnly)
    {
        var now = clock.GetUtcNow();
        var expires = now + Lifetime;
        var token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Issuer,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            Claims = new Dictionary<string, object> { [ReadOnlyClaim] = readOnly },
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        });
        return (token, expires);
    }

    /// <summary>Checks signature, issuer, audience and lifetime. <paramref name="readOnly"/> is meaningful only when the result is true.</summary>
    public bool TryValidate(string? token, out bool readOnly)
    {
        readOnly = false;
        if (string.IsNullOrWhiteSpace(token) || token.Count(c => c == '.') != 2)
        {
            return false;
        }

        var result = handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = Issuer,
            ValidAudience = Issuer,
            IssuerSigningKey = key,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.Zero,
            LifetimeValidator = (notBefore, expires, _, _) => (notBefore is null || notBefore <= clock.GetUtcNow()) && expires is not null && expires > clock.GetUtcNow(),
        }).GetAwaiter().GetResult();
        if (!result.IsValid)
        {
            return false;
        }

        readOnly = result.Claims.TryGetValue(ReadOnlyClaim, out var value) && value is true;
        return true;
    }
}
