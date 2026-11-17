using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Unsubscribe;

/// <summary>
/// Creates signed unsubscribe tokens and the RFC 8058 headers that carry them, and applies a token received at the endpoint
/// to the <see cref="ISuppressionList"/>. A token is the address plus an HMAC-SHA512 signature, so it cannot be forged
/// for another address and needs no storage; it does not expire, because an old message must still unsubscribe.
/// </summary>
/// <param name="options">The endpoint URL and signing key.</param>
/// <param name="suppressions">The list an unsubscribe is written to.</param>
public sealed class UnsubscribeLinks(IOptions<UnsubscribeOptions> options, ISuppressionList suppressions)
{
    /// <summary>The reason stored on the suppression list.</summary>
    public const string Reason = "unsubscribe";

    /// <summary>Creates the token for an address.</summary>
    /// <param name="address">The recipient address.</param>
    /// <returns>A URL-safe token.</returns>
    public string CreateToken(string address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        var payload = Encoding.UTF8.GetBytes(address.Trim().ToLower(CultureInfo.InvariantCulture));
        var signature = Sign(payload);
        return Base64Url(payload) + "." + Base64Url(signature);
    }

    /// <summary>Reads a token and checks its signature.</summary>
    /// <param name="token">The token from the request.</param>
    /// <param name="address">The address it was created for.</param>
    /// <returns><see langword="true"/> when the signature is valid.</returns>
    public bool TryReadToken(string? token, out string address)
    {
        address = string.Empty;
        var parts = token?.Split('.');
        if (parts is not { Length: 2 } || !TryFromBase64Url(parts[0], out var payload) || !TryFromBase64Url(parts[1], out var signature))
        {
            return false;
        }

        if (!CryptographicOperations.FixedTimeEquals(Sign(payload), signature))
        {
            return false;
        }

        address = Encoding.UTF8.GetString(payload);
        return true;
    }

    /// <summary>The two headers to add to a bulk message: <c>List-Unsubscribe</c> and <c>List-Unsubscribe-Post</c>.</summary>
    /// <param name="address">The recipient address.</param>
    /// <returns>The header names and values.</returns>
    public IReadOnlyDictionary<string, string> CreateHeaders(string address)
    {
        var url = options.Value.EndpointUrl;
        var separator = url.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["List-Unsubscribe"] = $"<{url}{separator}t={CreateToken(address)}>",
            ["List-Unsubscribe-Post"] = "List-Unsubscribe=One-Click",
        };
    }

    /// <summary>Suppresses the address in a valid token.</summary>
    /// <param name="token">The token from the request.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The address that was suppressed, or <see langword="null"/> when the token is not valid.</returns>
    public async Task<string?> UnsubscribeAsync(string? token, CancellationToken cancellationToken = default)
    {
        if (!TryReadToken(token, out var address))
        {
            return null;
        }

        await suppressions.AddAsync(address, Reason, cancellationToken).ConfigureAwait(false);
        return address;
    }

    private byte[] Sign(byte[] payload) => HMACSHA512.HashData(Encoding.UTF8.GetBytes(options.Value.SigningKey), payload);

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static bool TryFromBase64Url(string text, out byte[] bytes)
    {
        var padded = text.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + ((4 - (padded.Length % 4)) % 4), '=');
        try
        {
            bytes = Convert.FromBase64String(padded);
            return true;
        }
        catch (FormatException)
        {
            bytes = [];
            return false;
        }
    }
}
