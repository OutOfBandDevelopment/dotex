using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Hand-written repeatable scripts for an engine with no diff tool (Oracle, owner Q35 A), and the idempotent "ensure" scripts of any engine.
/// The plan is the ordered script list; scripts must be safe to run again. The same drop gate applies as for the diff tools.
/// </summary>
public sealed class ScriptFolderProvider(string engine, IScriptExecutor executor) : IDesiredStateProvider
{
    public const string Oracle = "oracle";

    public string Engine { get; } = engine;

    public Task<SchemaPlan> PlanAsync(DesiredSchema desired, DatabaseTarget target, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(desired);
        var statements = desired.Scripts.Select(s => s.Text).ToList();
        return Task.FromResult(new SchemaPlan(Engine, statements, PlanAnalyzer.Analyze(statements), []));
    }

    public async Task ApplyAsync(DesiredSchema desired, DatabaseTarget target, SchemaPlan plan, ApplyOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(desired);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(options);
        var refused = plan.Hazards.Where(h => !options.Permits(h)).ToList();
        if (refused.Count > 0)
        {
            throw new UnsafePlanException(refused);
        }

        foreach (var script in desired.Scripts)
        {
            await executor.ExecuteAsync(target, script.Name, script.Text, cancellationToken).ConfigureAwait(false);
        }
    }
}
