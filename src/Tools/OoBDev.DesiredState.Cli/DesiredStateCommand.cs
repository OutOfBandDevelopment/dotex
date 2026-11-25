using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OoBDev.Data.DesiredState;

namespace OoBDev.DesiredState.Cli;

/// <summary>
/// <c>desired-state plan|apply|pack</c>. Exit codes: 0 nothing to do or applied, 1 refused or hazards in the plan, 2 error, 3 plan has pending safe changes.
/// </summary>
internal static class DesiredStateCommand
{
    public const string Usage = """
        desired-state plan  --engine postgres|sqlserver|sqlite --scripts <dir> --database <name> [--host h] [--port p] [--user u] [--password-env NAME]
        desired-state apply (same options) [--allow-data-loss true] [--allow-type-change true]
        The password is read from the environment variable named by --password-env (default DESIRED_STATE_PASSWORD), never from the command line.
        desired-state pack  --source <dir> --name <n> --version <v> --out <file.zip>   (source: schema/, predeploy/, postdeploy/, refactorlog.json)
        For sqlite, --database is the file path. Exit: 0 done or nothing to do, 1 refused or hazards, 2 error, 3 plan has pending changes.
        """;

    public static async Task<int> RunAsync(string[] args, DesiredStateProviderFactory factory, Func<string, string?> environment, TextWriter output, TextWriter error, CancellationToken cancellationToken = default)
    {
        if (args.Length == 0 || args[0] is not ("plan" or "apply" or "pack"))
        {
            await error.WriteLineAsync(Usage).ConfigureAwait(false);
            return 2;
        }

        try
        {
            var config = new ConfigurationBuilder().AddCommandLine(args[1..]).Build();
            if (args[0] == "pack")
            {
                return await PackAsync(config, output).ConfigureAwait(false);
            }

            var engine = Required(config, "engine");
            var desired = DesiredSchema.FromDirectory(Required(config, "scripts"));
            var database = Required(config, "database");
            var passwordName = config["password-env"] ?? "DESIRED_STATE_PASSWORD";
            var target = engine.Equals(SqldefProvider.Sqlite, StringComparison.OrdinalIgnoreCase)
                ? DatabaseTarget.File(database)
                : new DatabaseTarget(config["host"] ?? "localhost", int.TryParse(config["port"], global::System.Globalization.CultureInfo.InvariantCulture, out var port) ? port : 0, database, config["user"], environment(passwordName));

            var provider = factory.Create(engine);
            var plan = await provider.PlanAsync(desired, target, cancellationToken).ConfigureAwait(false);
            await WritePlanAsync(plan, output).ConfigureAwait(false);

            if (args[0] == "plan")
            {
                return plan.IsEmpty ? 0 : plan.Hazards.Count > 0 ? 1 : 3;
            }

            var options = new ApplyOptions(Flag(config, "allow-data-loss"), Flag(config, "allow-type-change"));
            await provider.ApplyAsync(desired, target, plan, options, cancellationToken).ConfigureAwait(false);
            await output.WriteLineAsync(plan.IsEmpty ? "Nothing to apply." : "Applied.").ConfigureAwait(false);
            return 0;
        }
        catch (UnsafePlanException ex)
        {
            await error.WriteLineAsync(ex.Message + " Re-run with --allow-data-loss true or --allow-type-change true to permit it.").ConfigureAwait(false);
            return 1;
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or InvalidOperationException or NotSupportedException)
        {
            await error.WriteLineAsync(ex.Message).ConfigureAwait(false);
            return 2;
        }
    }

    private static async Task<int> PackAsync(IConfiguration config, TextWriter output)
    {
        var package = SchemaPackage.FromDirectory(Required(config, "source"), Required(config, "name"), Required(config, "version"));
        var path = Required(config, "out");
        await using (var stream = File.Create(path))
        {
            SchemaPackageFormat.Write(package, stream);
        }

        await output.WriteLineAsync($"Packed {package.Schema.Scripts.Count} schema, {package.PreDeploy.Scripts.Count} predeploy, {package.PostDeploy.Scripts.Count} postdeploy scripts and {package.Refactors.Count} refactors to {path}. Hash {package.Hash}").ConfigureAwait(false);
        return 0;
    }

    private static async Task WritePlanAsync(SchemaPlan plan, TextWriter output)
    {
        if (plan.IsEmpty)
        {
            await output.WriteLineAsync("No changes.").ConfigureAwait(false);
            return;
        }

        foreach (var statement in plan.Statements)
        {
            await output.WriteLineAsync(statement).ConfigureAwait(false);
        }

        foreach (var hazard in plan.Hazards)
        {
            await output.WriteLineAsync($"-- HAZARD {hazard.Kind}: {hazard.Message}").ConfigureAwait(false);
        }

        foreach (var skipped in plan.Skipped.Where(s => s.Length > 0))
        {
            await output.WriteLineAsync($"-- Skipped: {skipped}").ConfigureAwait(false);
        }
    }

    private static bool Flag(IConfiguration config, string name) => bool.TryParse(config[name], out var value) && value;

    private static string Required(IConfiguration config, string name) =>
        config[name] is { Length: > 0 } value ? value : throw new ArgumentException($"--{name} is required.", name);
}
