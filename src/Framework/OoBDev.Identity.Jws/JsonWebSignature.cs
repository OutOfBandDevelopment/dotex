using System;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OoBDev.Identity.Jws;

/// <summary>
/// Signs and verifies compact JSON Web Signatures (RFC 7515) with HS256 and RS256, written from the specification as a reference implementation.
/// Every verify method pins the algorithm the caller expects, so a token cannot choose its own (including <c>none</c>).
/// </summary>
public static class JsonWebSignature
{
    /// <summary>The <c>alg</c> value for HMAC with SHA-256.</summary>
    public const string Hs256 = "HS256";

    /// <summary>The <c>alg</c> value for RSASSA-PKCS1-v1_5 with SHA-256.</summary>
    public const string Rs256 = "RS256";

    /// <summary>RFC 7518 section 3.2: an HMAC key is at least as long as the hash output.</summary>
    public const int MinimumHs256KeyBytes = 32;

    /// <summary>RFC 7518 section 3.3: an RSA key is at least 2048 bits.</summary>
    public const int MinimumRsaKeyBits = 2048;

    /// <summary>
    /// Creates <c>header.payload.signature</c>, each part base64url without padding.
    /// </summary>
    /// <param name="headerJson">The JOSE header; its <c>alg</c> must be <c>HS256</c>.</param>
    /// <param name="payloadJson">The payload text, signed as its UTF-8 bytes.</param>
    /// <param name="key">The shared secret, at least 32 bytes.</param>
    /// <returns>The compact serialization.</returns>
    public static string SignHs256(string headerJson, string payloadJson, ReadOnlySpan<byte> key)
    {
        if (key.Length < MinimumHs256KeyBytes)
        {
            throw new ArgumentException($"An HS256 key needs at least {MinimumHs256KeyBytes} bytes.", nameof(key));
        }

        var signingInput = SigningInput(headerJson, payloadJson, Hs256);
        var signature = HMACSHA256.HashData(key, Encoding.ASCII.GetBytes(signingInput));
        return signingInput + "." + Base64Url.EncodeToString(signature);
    }

    /// <summary>
    /// Checks a compact token against the shared secret and returns its payload.
    /// </summary>
    /// <param name="token">The compact serialization.</param>
    /// <param name="key">The shared secret, at least 32 bytes.</param>
    /// <param name="payloadJson">The payload text when the signature is valid; otherwise empty.</param>
    /// <returns><c>false</c> for a changed part, a wrong key, malformed input or an <c>alg</c> other than <c>HS256</c>.</returns>
    public static bool TryVerifyHs256(string token, ReadOnlySpan<byte> key, out string payloadJson)
    {
        payloadJson = string.Empty;
        if (key.Length < MinimumHs256KeyBytes || !TrySplit(token, Hs256, out var signingInput, out var payloadPart, out var signature))
        {
            return false;
        }

        var expected = HMACSHA256.HashData(key, Encoding.ASCII.GetBytes(signingInput));
        return CryptographicOperations.FixedTimeEquals(expected, signature) && TryDecodeText(payloadPart, out payloadJson);
    }

    /// <summary>
    /// Creates <c>header.payload.signature</c> signed with an RSA private key.
    /// </summary>
    /// <param name="headerJson">The JOSE header; its <c>alg</c> must be <c>RS256</c>.</param>
    /// <param name="payloadJson">The payload text, signed as its UTF-8 bytes.</param>
    /// <param name="privateKey">An RSA key of at least 2048 bits that holds the private parameters.</param>
    /// <returns>The compact serialization.</returns>
    public static string SignRs256(string headerJson, string payloadJson, RSA privateKey)
    {
        ArgumentNullException.ThrowIfNull(privateKey);
        if (privateKey.KeySize < MinimumRsaKeyBits)
        {
            throw new ArgumentException($"An RS256 key needs at least {MinimumRsaKeyBits} bits.", nameof(privateKey));
        }

        var signingInput = SigningInput(headerJson, payloadJson, Rs256);
        var signature = privateKey.SignData(Encoding.ASCII.GetBytes(signingInput), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return signingInput + "." + Base64Url.EncodeToString(signature);
    }

    /// <summary>
    /// Checks a compact token against an RSA public key and returns its payload.
    /// </summary>
    /// <param name="token">The compact serialization.</param>
    /// <param name="publicKey">An RSA key of at least 2048 bits.</param>
    /// <param name="payloadJson">The payload text when the signature is valid; otherwise empty.</param>
    /// <returns><c>false</c> for a changed part, a wrong key, malformed input or an <c>alg</c> other than <c>RS256</c>.</returns>
    public static bool TryVerifyRs256(string token, RSA publicKey, out string payloadJson)
    {
        ArgumentNullException.ThrowIfNull(publicKey);
        payloadJson = string.Empty;
        if (publicKey.KeySize < MinimumRsaKeyBits || !TrySplit(token, Rs256, out var signingInput, out var payloadPart, out var signature))
        {
            return false;
        }

        var valid = publicKey.VerifyData(Encoding.ASCII.GetBytes(signingInput), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return valid && TryDecodeText(payloadPart, out payloadJson);
    }

    private static string SigningInput(string headerJson, string payloadJson, string algorithm)
    {
        ArgumentNullException.ThrowIfNull(headerJson);
        ArgumentNullException.ThrowIfNull(payloadJson);
        if (!HeaderIsAcceptable(headerJson, algorithm))
        {
            throw new ArgumentException($"The header must be a JSON object with \"alg\": \"{algorithm}\" and no \"crit\".", nameof(headerJson));
        }

        return Base64Url.EncodeToString(Encoding.UTF8.GetBytes(headerJson)) + "." + Base64Url.EncodeToString(Encoding.UTF8.GetBytes(payloadJson));
    }

    private static bool TrySplit(string token, string algorithm, out string signingInput, out string payloadPart, out byte[] signature)
    {
        signingInput = payloadPart = string.Empty;
        signature = [];
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        var parts = token.Split('.');
        if (parts.Length != 3 || !TryDecodeText(parts[0], out var headerJson) || !HeaderIsAcceptable(headerJson, algorithm))
        {
            return false;
        }

        try
        {
            signature = Base64Url.DecodeFromChars(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        signingInput = parts[0] + "." + parts[1];
        payloadPart = parts[1];
        return true;
    }

    private static bool HeaderIsAcceptable(string headerJson, string algorithm)
    {
        try
        {
            using var document = JsonDocument.Parse(headerJson);
            var root = document.RootElement;
            return root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("alg", out var alg)
                && alg.ValueKind == JsonValueKind.String
                && string.Equals(alg.GetString(), algorithm, StringComparison.Ordinal)
                && !root.TryGetProperty("crit", out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryDecodeText(string part, out string text)
    {
        text = string.Empty;
        try
        {
            text = new UTF8Encoding(false, true).GetString(Base64Url.DecodeFromChars(part));
            return true;
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            return false;
        }
    }
}
