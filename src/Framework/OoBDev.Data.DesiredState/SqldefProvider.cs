using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Wraps a sqldef binary (<c>psqldef</c>, <c>mssqldef</c>, <c>sqlite3def</c>; MIT) for one engine. sqldef already skips drops without
/// <c>--enable-drop</c>; this wrapper adds the type-change check and a plan-then-apply step that refuses unpermitted hazards (owner Q34 A).
/// </summary>
public sealed class SqldefProvider(string engine, string executable, IToolRunner runner) : IDesiredStateProvider
{
    public const string Postgres = "postgres";
    public const string SqlServer = "sqlserver";
    public const string Sqlite = "sqlite";

    public string Engine { get; } = engine;

    public static SqldefProvider ForPostgres(IToolRunner runner, string executable = "psqldef") => new(Postgres, executable, runner);

    public static SqldefProvider ForSqlServer(IToolRunner runner, string executable = "mssqldef") => new(SqlServer, executable, runner);

    public static SqldefProvider ForSqlite(IToolRunner runner, string executable = "sqlite3def") => new(Sqlite, executable, runner);

    public async Task<SchemaPlan> PlanAsync(DesiredSchema desired, DatabaseTarget target, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(desired);
        ArgumentNullException.ThrowIfNull(target);
        var result = await RunAsync(desired, target, ["--dry-run"], cancellationToken).ConfigureAwait(false);
        var (statements, skipped) = PlanAnalyzer.Parse(result.StandardOutput);
        return new SchemaPlan(Engine, statements, PlanAnalyzer.Analyze(statements), skipped);
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

        if (plan.IsEmpty && !(options.AllowDataLoss && plan.Skipped.Count > 0))
        {
            return;
        }

        // sqldef skips drops in its plan, so a permitted drop is applied even when the plan has no statements.
        string[] extra = options.AllowDataLoss ? ["--enable-drop"] : [];
        await RunAsync(desired, target, extra, cancellationToken).ConfigureAwait(false);
    }

    internal IReadOnlyList<string> BuildArguments(DatabaseTarget target, IEnumerable<string> extra)
    {
        var args = new List<string>();
        if (Engine != Sqlite)
        {
            args.AddRange(["-h", target.Host, "-p", target.Port.ToString(CultureInfo.InvariantCulture)]);
            if (!string.IsNullOrEmpty(target.User))
            {
                args.AddRange(["-U", target.User]);
            }
        }

        args.AddRange(extra);
        args.Add(target.Database);
        return args;
    }

    private async Task<ToolResult> RunAsync(DesiredSchema desired, DatabaseTarget target, IEnumerable<string> extra, CancellationToken cancellationToken)
    {
        var environment = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!string.IsNullOrEmpty(target.Password))
        {
            environment[Engine == Postgres ? "PGPASSWORD" : "MSSQL_PWD"] = target.Password;
        }

        var result = await runner.RunAsync(executable, BuildArguments(target, extra), desired.Sql, environment, cancellationToken).ConfigureAwait(false);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"{executable} exited with {result.ExitCode}: {result.StandardError.Trim()}");
        }

        return result;
    }
}
