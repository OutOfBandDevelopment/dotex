using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OoBDev.DevTerm;

/// <summary>
/// The trust model for out-of-process plugins: such a plugin runs only after
/// the user approves it, optionally once per <see cref="PluginHash"/> so an unchanged plugin isn't asked about again and any
/// edit (a swapped script, an updated binary) asks again. No signing. The approvals live in
/// <c>plugin-approvals.json</c> under <see cref="DevTermHome"/>.
/// </summary>
public static class PluginTrust
{
    private static readonly global::System.Threading.Lock _gate = new();
    private static readonly JsonSerializerOptions _writeOptions = new() { WriteIndented = true };

    /// <summary>The front end's prompt; null means "never ask" (only remembered approvals run).</summary>
    public static IPluginApprover? Approver { get; set; }

    /// <summary>Where approvals are remembered; defaults to <c>plugin-approvals.json</c> in <see cref="DevTermHome.Root"/>.</summary>
    public static string StorePath => Path.Combine(DevTermHome.Root, "plugin-approvals.json");

    /// <summary>Whether this exact content (<paramref name="hash"/>) was approved with <see cref="PluginApprovalChoice.Always"/>.</summary>
    public static bool IsApproved(string name, string hash)
    {
        lock (_gate)
        {
            return Read().Any(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase) && string.Equals(e.Hash, hash, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static void Remember(string name, string hash)
    {
        lock (_gate)
        {
            var entries = Read();
            entries.RemoveAll(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
            entries.Add(new Entry { Name = name, Hash = hash, ApprovedAt = TimeProvider.System.GetUtcNow() });
            Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
            File.WriteAllText(StorePath, JsonSerializer.Serialize(new Store { Approved = entries }, _writeOptions));
        }
    }

    /// <summary>Every remembered approval, oldest first: the plugin name, the approved content hash and when it was approved.</summary>
    public static IReadOnlyList<PluginApproval> Approvals()
    {
        lock (_gate)
        {
            return [.. Read().OrderBy(e => e.ApprovedAt).Select(e => new PluginApproval(e.Name, e.Hash, e.ApprovedAt))];
        }
    }

    /// <summary>Walks the remembered approvals one at a time, forgetting those <paramref name="ask"/> says to; returns how many were forgotten.</summary>
    public static int Review(Func<PluginApproval, ApprovalReviewChoice> ask)
    {
        ArgumentNullException.ThrowIfNull(ask);
        var forgotten = 0;
        foreach (var approval in Approvals())
        {
            var choice = ask(approval);
            if (choice == ApprovalReviewChoice.Stop)
            {
                break;
            }

            if (choice == ApprovalReviewChoice.Forget)
            {
                Forget(approval.Name);
                forgotten++;
            }
        }

        return forgotten;
    }

    /// <summary>Forgets a plugin's remembered approval.</summary>
    public static void Forget(string name)
    {
        lock (_gate)
        {
            var entries = Read();
            if (entries.RemoveAll(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                File.WriteAllText(StorePath, JsonSerializer.Serialize(new Store { Approved = entries }, _writeOptions));
            }
        }
    }

    /// <summary>Decides whether the plugin may run, asking (and remembering) as needed. Never throws.</summary>
    public static bool Authorise(PluginApprovalRequest request)
    {
        if (IsApproved(request.Name, request.Hash))
        {
            return true;
        }

        var choice = Approver?.Ask(request) ?? PluginApprovalChoice.Deny;
        if (choice == PluginApprovalChoice.Always)
        {
            try
            {
                Remember(request.Name, request.Hash);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Couldn't save the approval: still run this once, and ask again next time.
            }
        }

        return choice != PluginApprovalChoice.Deny;
    }

    private static List<Entry> Read()
    {
        try
        {
            return File.Exists(StorePath) ? [.. JsonSerializer.Deserialize<Store>(File.ReadAllText(StorePath))?.Approved ?? []] : [];
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    private sealed class Store
    {
        public List<Entry> Approved { get; set; } = [];
    }

    private sealed class Entry
    {
        public string Name { get; set; } = string.Empty;

        public string Hash { get; set; } = string.Empty;

        public DateTimeOffset ApprovedAt { get; set; }
    }
}
