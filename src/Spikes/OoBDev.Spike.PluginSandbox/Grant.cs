using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>The subset of a manifest's requests that a person approved (REQ-002), with who and when.</summary>
public sealed record Grant(string PluginId, IReadOnlyList<FilePermission> Files, IReadOnlyList<NetworkPermission> Network, string ApprovedBy, DateTimeOffset ApprovedAt)
{
    /// <summary>A stable hash of the approved permissions; it names the shared-instance mutex so callers with different grants never share a plugin.</summary>
    public string Hash
    {
        get
        {
            var lines = Files.Select(f => $"file|{f.Path}|{f.Access}")
                .Concat(Network.Select(n => $"net|{n.Host.ToLowerInvariant()}|{n.Port}"))
                .Order(StringComparer.Ordinal);
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(PluginId + "\n" + string.Join('\n', lines)))).ToLowerInvariant();
        }
    }
}
