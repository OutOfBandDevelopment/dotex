using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Runs ensure scripts (extensions, queues, event streams: objects a schema diff cannot model) in file-name order. Scripts must be idempotent and run every time;
/// a script named <c>*.once.sql</c> is not idempotent by itself and runs only when its name and content hash are not yet recorded in the <see cref="IAppliedScriptStore"/>.
/// </summary>
public sealed class EnsureScriptRunner(IScriptExecutor executor, IAppliedScriptStore store)
{
    public const string OnceSuffix = ".once.sql";

    public async Task<EnsureReport> RunAsync(DesiredSchema scripts, DatabaseTarget target, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scripts);
        ArgumentNullException.ThrowIfNull(target);
        var ran = new List<string>();
        var skipped = new List<string>();
        foreach (var script in scripts.Scripts.OrderBy(s => s.Name, StringComparer.Ordinal))
        {
            var once = script.Name.EndsWith(OnceSuffix, StringComparison.OrdinalIgnoreCase);
            var hash = Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(script.Text)));
            if (once && await store.HasAppliedAsync(target, script.Name, hash, cancellationToken).ConfigureAwait(false))
            {
                skipped.Add(script.Name);
                continue;
            }

            await executor.ExecuteAsync(target, script.Name, script.Text, cancellationToken).ConfigureAwait(false);
            if (once)
            {
                await store.RecordAsync(target, script.Name, hash, cancellationToken).ConfigureAwait(false);
            }

            ran.Add(script.Name);
        }

        return new EnsureReport(ran, skipped);
    }
}
