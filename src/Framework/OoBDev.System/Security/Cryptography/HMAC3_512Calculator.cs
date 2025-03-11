using System;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.System.Security.Cryptography;

public class HMAC3_512Calculator : IHMACCalculator
{
    public ReadOnlySpan<byte> Calculate(string secret, string message)
    {
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var uriBytes = Encoding.UTF8.GetBytes(message);
        using var hmac = new HMACSHA3_512(secretBytes);
        var hashBytes = hmac.ComputeHash(uriBytes);
        return hashBytes;
    }

    public string Encode(ReadOnlySpan<byte> bytes)
    {
        var hashInBase64 = Convert.ToBase64String(bytes);
        var hashInBase64URIencoded = hashInBase64; ;
        return hashInBase64URIencoded;
    }

    public string CalculateAndEncode(string secret, string message) => Encode(Calculate(secret, message));
}
