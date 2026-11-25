using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Deploys a <see cref="SchemaPackage"/> the way a dacpac is deployed: pre-deploy scripts, unapplied refactor entries, the schema plan and apply (gates included),
/// post-deploy scripts, then a history record with the package hash. Records live in the <see cref="IAppliedScriptStore"/>
/// (<c>refactor:{id}</c> and <c>package:{name}</c>); use <see cref="AdoAppliedScriptStore"/> so they survive the process.
/// </summary>
public sealed partial class SchemaPackageDeployer(
    DesiredStateProviderFactory factory,
    EnsureScriptRunner scripts,
    IScriptExecutor executor,
    IAppliedScriptStore store)
{
    public async Task<PackageDeployResult> DeployAsync(SchemaPackage package, string engine, DatabaseTarget target, ApplyOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(target);
        var provider = factory.Create(engine);
        var pre = await scripts.RunAsync(package.PreDeploy, target, cancellationToken).ConfigureAwait(false);

        var plan = await provider.PlanAsync(package.Schema, target, cancellationToken).ConfigureAwait(false);
        var ran = new List<string>();
        var recorded = new List<string>();
        foreach (var entry in package.Refactors)
        {
            var name = $"refactor:{entry.Id}";
            var hash = Hash(entry.Sql);
            if (await store.HasAppliedAsync(target, name, hash, cancellationToken).ConfigureAwait(false))
            {
                continue;
            }

            if (CreatesTable(plan, entry.Table))
            {
                recorded.Add(entry.Id);
            }
            else
            {
                await executor.ExecuteAsync(target, name, entry.Sql, cancellationToken).ConfigureAwait(false);
                ran.Add(entry.Id);
            }

            await store.RecordAsync(target, name, hash, cancellationToken).ConfigureAwait(false);
        }

        if (ran.Count > 0)
        {
            plan = await provider.PlanAsync(package.Schema, target, cancellationToken).ConfigureAwait(false);
        }

        await provider.ApplyAsync(package.Schema, target, plan, options ?? new ApplyOptions(), cancellationToken).ConfigureAwait(false);
        var post = await scripts.RunAsync(package.PostDeploy, target, cancellationToken).ConfigureAwait(false);

        var historyName = $"package:{package.Name}";
        var first = !await store.HasAppliedAsync(target, historyName, package.Hash, cancellationToken).ConfigureAwait(false);
        if (first)
        {
            await store.RecordAsync(target, historyName, package.Hash, cancellationToken).ConfigureAwait(false);
        }

        return new PackageDeployResult(ran, recorded, plan, pre, post, first);
    }

    private static string Hash(string text) => Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(text)));

    private static bool CreatesTable(SchemaPlan plan, string table) =>
        plan.Statements.Any(s => CreateTable().Match(Unquote(s)) is { Success: true } m && string.Equals(m.Groups["t"].Value, table, StringComparison.OrdinalIgnoreCase));

    private static string Unquote(string sql) => sql.Replace("\"", string.Empty, StringComparison.Ordinal).Replace("`", string.Empty, StringComparison.Ordinal)
        .Replace("[", string.Empty, StringComparison.Ordinal).Replace("]", string.Empty, StringComparison.Ordinal);

    [GeneratedRegex(@"^\s*CREATE\s+TABLE\s+(IF\s+NOT\s+EXISTS\s+)?(\w+\.)?(?<t>\w+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, 200)]
    private static partial Regex CreateTable();
}
