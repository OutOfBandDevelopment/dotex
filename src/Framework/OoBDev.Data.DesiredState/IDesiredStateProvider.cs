using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>Plans and applies the difference between a folder of CREATE scripts and a database for one engine.</summary>
public interface IDesiredStateProvider
{
    /// <summary>The engine key: <c>postgres</c>, <c>sqlserver</c>, <c>sqlite</c> or <c>oracle</c>.</summary>
    string Engine { get; }

    /// <summary>Computes the plan without changing the database.</summary>
    Task<SchemaPlan> PlanAsync(DesiredSchema desired, DatabaseTarget target, CancellationToken cancellationToken = default);

    /// <summary>Applies a plan; throws <see cref="UnsafePlanException"/> before running anything when it has hazards the options do not permit.</summary>
    Task ApplyAsync(DesiredSchema desired, DatabaseTarget target, SchemaPlan plan, ApplyOptions options, CancellationToken cancellationToken = default);
}
