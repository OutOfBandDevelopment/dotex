using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>
/// Holds the approved grants and the audit trail (in memory in the spike; a real store is persisted and protected). A grant can only be a
/// subset of what the manifest requested, so an approver cannot hand out more than the package asked for.
/// </summary>
public sealed class GrantStore(TimeProvider time)
{
    private readonly TimeProvider _time = time ?? throw new ArgumentNullException(nameof(time));
    private readonly Lock _gate = new();
    private readonly Dictionary<string, Grant> _grants = new(StringComparer.Ordinal);
    private readonly List<AuditEntry> _audit = [];

    /// <summary>Every audit entry so far, oldest first.</summary>
    public IReadOnlyList<AuditEntry> Audit
    {
        get
        {
            lock (_gate)
            {
                return [.. _audit];
            }
        }
    }

    /// <summary>Stores <paramref name="approved"/> for the verified manifest; anything not requested is refused with an <see cref="ArgumentException"/>.</summary>
    public Grant Approve(PluginManifest manifest, PluginPermissions approved, string approver)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(approved);
        ArgumentException.ThrowIfNullOrWhiteSpace(approver);
        var files = approved.Files ?? [];
        var network = approved.Network ?? [];
        foreach (var file in files.Where(f => !manifest.Permissions.Files?.Any(r => Covers(r, f)) ?? true))
        {
            throw new ArgumentException($"The manifest did not request {file.Path} ({file.Access}).", nameof(approved));
        }

        foreach (var net in network.Where(n => !manifest.Permissions.Network?.Contains(n) ?? true))
        {
            throw new ArgumentException($"The manifest did not request {net.Host}:{net.Port}.", nameof(approved));
        }

        var grant = new Grant(manifest.Id, [.. files], [.. network], approver, _time.GetUtcNow());
        lock (_gate)
        {
            _grants[manifest.Id] = grant;
            _audit.Add(new AuditEntry(grant.ApprovedAt, manifest.Id, "approve", $"{files.Count} folders, {network.Count} hosts by {approver}", true, grant.Hash));
        }

        return grant;
    }

    /// <summary>The grant for a plugin, or null when none was approved (then the plugin has no files and no network).</summary>
    public Grant? Find(string pluginId)
    {
        lock (_gate)
        {
            return _grants.GetValueOrDefault(pluginId);
        }
    }

    /// <summary>Withdraws a grant; later broker calls are refused.</summary>
    public bool Revoke(string pluginId)
    {
        lock (_gate)
        {
            var removed = _grants.Remove(pluginId);
            if (removed)
            {
                _audit.Add(new AuditEntry(_time.GetUtcNow(), pluginId, "revoke", string.Empty, true, string.Empty));
            }

            return removed;
        }
    }

    internal void Record(string pluginId, string action, string target, bool allowed, string reason)
    {
        lock (_gate)
        {
            _audit.Add(new AuditEntry(_time.GetUtcNow(), pluginId, action, target, allowed, reason));
        }
    }

    // A read-only request can be approved read-only; a read-write request can be approved either way; the same folder only.
    private static bool Covers(FilePermission requested, FilePermission approved) =>
        string.Equals(requested.Path, approved.Path, StringComparison.Ordinal)
        && (string.Equals(requested.Access, approved.Access, StringComparison.Ordinal) || (requested.Access == "rw" && approved.Access == "ro"));
}
