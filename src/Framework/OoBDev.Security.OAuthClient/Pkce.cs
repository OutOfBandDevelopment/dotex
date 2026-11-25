using System;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.Security.OAuthClient;

/// <summary>Proof Key for Code Exchange (RFC 7636), method S256.</summary>
public static class Pkce
{
    /// <summary>Makes a code verifier: 32 random bytes, base64url, 43 characters.</summary>
    /// <returns>The verifier.</returns>
    public static string CreateVerifier() => Base64Url(RandomNumberGenerator.GetBytes(32));

    /// <summary>The S256 challenge for a verifier.</summary>
    /// <param name="verifier">The code verifier.</param>
    /// <returns>The challenge: base64url of the SHA-256 of the verifier.</returns>
    public static string ChallengeFor(string verifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(verifier);
        return Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
    }

    /// <summary>A random value for the <c>state</c> parameter.</summary>
    /// <returns>16 random bytes, base64url.</returns>
    public static string CreateState() => Base64Url(RandomNumberGenerator.GetBytes(16));

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
