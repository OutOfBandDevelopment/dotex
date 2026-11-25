using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Brings the database of an EF Core context to the final state of its model, in place of <c>Database.MigrateAsync()</c>: the create script of the model is the
/// desired state, the provider plans the difference, the gates refuse unpermitted drops and type changes, and ensure scripts (data changes as <c>*.once.sql</c>) run afterwards.
/// PostgreSQL and SQLite files only; the tool is another process, so an in-memory SQLite database is refused.
/// </summary>
public sealed class EfDesiredStateDeployer(DesiredStateProviderFactory factory, IServiceProvider services)
{
    /// <summary>Plans and applies the model of <paramref name="context"/>.</summary>
    /// <param name="context">The context whose connection string and model are used.</param>
    /// <param name="options">The hazards the caller permits; none by default.</param>
    /// <param name="ensureScripts">Scripts to run after the schema, or <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <exception cref="UnsafePlanException">The plan has hazards the options do not permit; nothing was run.</exception>
    public async Task<EfDesiredStateResult> EnsureAsync(DbContext context, ApplyOptions? options = null, DesiredSchema? ensureScripts = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var engine = DbContextTarget.EngineOf(context);
        var target = DbContextTarget.TargetOf(context, engine);
        var desired = new DesiredSchema([new DesiredScript($"{context.GetType().Name}.sql", DbContextTarget.ScriptOf(context))]);
        var provider = factory.Create(engine);

        var plan = await provider.PlanAsync(desired, target, cancellationToken).ConfigureAwait(false);
        await provider.ApplyAsync(desired, target, plan, options ?? new ApplyOptions(), cancellationToken).ConfigureAwait(false);

        EnsureReport? report = null;
        if (ensureScripts is not null)
        {
            report = await services.GetRequiredService<EnsureScriptRunner>().RunAsync(ensureScripts, target, cancellationToken).ConfigureAwait(false);
        }

        return new EfDesiredStateResult(plan, !plan.IsEmpty || ((options?.AllowDataLoss ?? false) && plan.Skipped.Count > 0), report);
    }
}
