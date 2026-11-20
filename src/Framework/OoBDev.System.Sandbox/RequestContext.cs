using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>
/// The immutable, minimised assertion about the caller that a plugin receives instead of credentials (REQ-012). The
/// <see cref="Id"/> is the SHA-256 of the canonical JSON of every other field, so any change shows up as a different id.
/// </summary>
public sealed class RequestContext
{
    private RequestContext(string subject, string tenant, string audience, IReadOnlyDictionary<string, string> claims, IReadOnlyList<string> rights, long issuedAt, long expiresAt, string traceId)
    {
        Subject = subject;
        Tenant = tenant;
        Audience = audience;
        Claims = claims;
        Rights = rights;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
        TraceId = traceId;
        Canonical = BuildCanonical();
        Id = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Canonical))).ToLowerInvariant();
    }

    /// <summary>SHA-256 of <see cref="Canonical"/>, lower-case hex.</summary>
    public string Id { get; }

    /// <summary>The stable subject of the validated token.</summary>
    public string Subject { get; }

    /// <summary>The tenant the request runs in.</summary>
    public string Tenant { get; }

    /// <summary>The plugin id this context is for.</summary>
    public string Audience { get; }

    /// <summary>Only the claims the plugin's manifest declared.</summary>
    public IReadOnlyDictionary<string, string> Claims { get; }

    /// <summary>Application rights (not raw roles), sorted.</summary>
    public IReadOnlyList<string> Rights { get; }

    /// <summary>Issue time, Unix seconds.</summary>
    public long IssuedAt { get; }

    /// <summary>Expiry, Unix seconds.</summary>
    public long ExpiresAt { get; }

    /// <summary>Correlation id for traces and audit.</summary>
    public string TraceId { get; }

    /// <summary>The canonical JSON that <see cref="Id"/> hashes: keys sorted, no whitespace.</summary>
    public string Canonical { get; }

    /// <summary>
    /// Builds a context from what the host already validated. Claims not in <paramref name="declaredClaims"/> are dropped; the token
    /// itself is never an input, so it cannot leak through this type.
    /// </summary>
    public static RequestContext Create(string subject, string tenant, string pluginId, IEnumerable<KeyValuePair<string, string>> validatedClaims, IEnumerable<string> declaredClaims, IEnumerable<string> rights, DateTimeOffset issuedAt, TimeSpan lifetime, string traceId)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(pluginId);
        if (lifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), "A context must expire in the future.");
        }

        var declared = new HashSet<string>(declaredClaims, StringComparer.Ordinal);
        var claims = validatedClaims
            .Where(c => declared.Contains(c.Key))
            .OrderBy(c => c.Key, StringComparer.Ordinal)
            .ToDictionary(c => c.Key, c => c.Value, StringComparer.Ordinal);
        var sortedRights = rights.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        return new RequestContext(subject, tenant, pluginId, claims, sortedRights, issuedAt.ToUnixTimeSeconds(), issuedAt.Add(lifetime).ToUnixTimeSeconds(), traceId);
    }

    /// <summary>True while <paramref name="now"/> is inside the validity window.</summary>
    public bool IsValidAt(DateTimeOffset now) => now.ToUnixTimeSeconds() >= IssuedAt && now.ToUnixTimeSeconds() < ExpiresAt;

    /// <summary>The JSON a plugin receives: the canonical fields plus the id.</summary>
    public string ToJson() => "{\"id\":" + JsonSerializer.Serialize(Id) + "," + Canonical[1..];

    private string BuildCanonical()
    {
        var builder = new StringBuilder("{");
        builder.Append("\"aud\":").Append(JsonSerializer.Serialize(Audience));
        builder.Append(",\"claims\":{");
        builder.Append(string.Join(",", Claims.OrderBy(c => c.Key, StringComparer.Ordinal).Select(c => JsonSerializer.Serialize(c.Key) + ":" + JsonSerializer.Serialize(c.Value))));
        builder.Append("},\"exp\":").Append(ExpiresAt.ToString(CultureInfo.InvariantCulture));
        builder.Append(",\"iat\":").Append(IssuedAt.ToString(CultureInfo.InvariantCulture));
        builder.Append(",\"rights\":[").Append(string.Join(",", Rights.Select(r => JsonSerializer.Serialize(r)))).Append(']');
        builder.Append(",\"sub\":").Append(JsonSerializer.Serialize(Subject));
        builder.Append(",\"tenant\":").Append(JsonSerializer.Serialize(Tenant));
        builder.Append(",\"traceId\":").Append(JsonSerializer.Serialize(TraceId));
        builder.Append('}');
        return builder.ToString();
    }
}
