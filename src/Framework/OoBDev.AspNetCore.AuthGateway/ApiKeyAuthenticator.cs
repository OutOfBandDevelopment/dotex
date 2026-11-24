using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.AspNetCore.AuthGateway;

public sealed class ApiKeyAuthenticator(IOptions<AuthGatewayOptions> options) : IInboundAuthenticator
{
    public InboundScheme Scheme => InboundScheme.ApiKey;

    public GatewayCaller? Authenticate(HttpContext context)
    {
        var settings = options.Value;
        var key = context.Request.Headers[settings.ApiKeyHeader].ToString();
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        var hash = SHA512.HashData(Encoding.UTF8.GetBytes(key));
        GatewayCaller? found = null;
        foreach (var entry in settings.ApiKeys)
        {
            // every entry is compared so timing does not reveal which one matched
            if (TryParse(entry.Sha512, out var expected) && CryptographicOperations.FixedTimeEquals(hash, expected))
            {
                found = new GatewayCaller(InboundScheme.ApiKey, entry.Subject);
            }
        }

        return found;
    }

    private static bool TryParse(string hex, out byte[] bytes)
    {
        try
        {
            bytes = Convert.FromHexString(hex);
            return bytes.Length == SHA512.HashSizeInBytes;
        }
        catch (FormatException)
        {
            bytes = [];
            return false;
        }
    }
}
