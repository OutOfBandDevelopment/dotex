using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Writes and reads the signed keyset cursor: <c>base64url(payload).base64url(HMAC-SHA256)</c>. A tampered cursor, a cursor of another kind or sort, or a cursor signed with another key is a 400.
/// </summary>
/// <param name="options">The options holding the signing key.</param>
public sealed class CursorCodec(IOptions<QueryMetadataOptions> options)
{
    private const string Invalid = "The after cursor is not valid.";

    /// <summary>Signs a cursor.</summary>
    /// <param name="state">What the cursor holds.</param>
    /// <returns>The opaque text.</returns>
    public string Encode(CursorState state)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(state);
        return Base64Url(payload) + "." + Base64Url(Sign(payload));
    }

    /// <summary>Verifies and reads a cursor.</summary>
    /// <param name="after">The cursor text from the request.</param>
    /// <param name="kind">The kind being queried.</param>
    /// <param name="sort">The normalized sort of this request.</param>
    /// <returns>The state, or null when there is no cursor.</returns>
    /// <exception cref="QueryException">The cursor is not valid for this request.</exception>
    public CursorState? Decode(string? after, string kind, string sort)
    {
        if (string.IsNullOrEmpty(after))
            return null;

        var dot = after.IndexOf('.', StringComparison.Ordinal);
        if (dot < 1)
            throw new QueryException(Invalid);

        byte[] payload;
        byte[] signature;
        try
        {
            payload = FromBase64Url(after[..dot]);
            signature = FromBase64Url(after[(dot + 1)..]);
        }
        catch (FormatException)
        {
            throw new QueryException(Invalid);
        }

        if (!CryptographicOperations.FixedTimeEquals(Sign(payload), signature))
            throw new QueryException(Invalid);

        CursorState? state;
        try
        {
            state = JsonSerializer.Deserialize<CursorState>(payload);
        }
        catch (JsonException)
        {
            throw new QueryException(Invalid);
        }

        if (state is null || state.Values is null)
            throw new QueryException(Invalid);
        if (!string.Equals(state.Kind, kind, StringComparison.OrdinalIgnoreCase) || state.Sort != sort)
            throw new QueryException("The after cursor belongs to a different kind or sort.");
        return state;
    }

    private byte[] Sign(byte[] payload) => HMACSHA256.HashData(Encoding.UTF8.GetBytes(options.Value.CursorKey), payload);

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] FromBase64Url(string text)
    {
        var padded = text.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + ((4 - (padded.Length % 4)) % 4), '=');
        return Convert.FromBase64String(padded);
    }
}
