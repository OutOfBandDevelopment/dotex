using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>Process-lifetime store for tests and one-shot tools.</summary>
public sealed class InMemoryAppliedScriptStore : IAppliedScriptStore
{
    private readonly ConcurrentDictionary<string, bool> _applied = new(System.StringComparer.Ordinal);

    public Task<bool> HasAppliedAsync(DatabaseTarget target, string scriptName, string hash, CancellationToken cancellationToken = default) =>
        Task.FromResult(_applied.ContainsKey(Key(target, scriptName, hash)));

    public Task RecordAsync(DatabaseTarget target, string scriptName, string hash, CancellationToken cancellationToken = default)
    {
        _applied[Key(target, scriptName, hash)] = true;
        return Task.CompletedTask;
    }

    private static string Key(DatabaseTarget target, string scriptName, string hash) => $"{target.Host}:{target.Port}/{target.Database}|{scriptName}|{hash}";
}
