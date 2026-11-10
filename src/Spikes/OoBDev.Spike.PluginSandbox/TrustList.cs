using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>The publisher keys the host trusts, by key id. A package signed by any other key is refused.</summary>
public sealed class TrustList
{
    private readonly Dictionary<string, byte[]> _keys = new(StringComparer.Ordinal);

    /// <summary>Trusts <paramref name="keyId"/> with its public key (SubjectPublicKeyInfo).</summary>
    public void Trust(string keyId, byte[] subjectPublicKeyInfo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyId);
        ArgumentNullException.ThrowIfNull(subjectPublicKeyInfo);
        _keys[keyId] = subjectPublicKeyInfo;
    }

    /// <summary>Removes a key (a revoked publisher).</summary>
    public bool Revoke(string keyId) => _keys.Remove(keyId);

    internal ECDsa? Open(string keyId)
    {
        if (!_keys.TryGetValue(keyId, out var spki))
        {
            return null;
        }

        var key = ECDsa.Create();
        key.ImportSubjectPublicKeyInfo(spki, out _);
        return key;
    }
}
