using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.DataLoader.EntityFrameworkCore;

namespace OoBDev.DataLoader.Cli;

/// <summary>The command behind <c>data-loader</c>, separated from <c>Main</c> so tests can run it.</summary>
public static class LoaderCommand
{
    private const string _usage = """
        data-loader --input rows.csv --key id [--format csv|json] [--dataset name] [--duplicates skip|replace|fail] [--stop-on-error true] [--batch-size 100] [--sqlite store.db]
        Without --sqlite the load is a dry run into memory: it validates the file and reports what would be written.
        With --sqlite the rows are written to that SQLite file (created when missing), keyed by --dataset (default: the file name) and --key.
        Exit code 0 clean, 1 row errors, 2 usage or file error.
        """;

    /// <summary>Runs one load and writes the report to <paramref name="output"/>.</summary>
    public static async Task<int> RunAsync(string[] args, TextWriter output, TextWriter error, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        var config = new ConfigurationBuilder().AddCommandLine(args).Build();
        var input = config["input"];
        var key = config["key"];
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(key))
        {
            await error.WriteLineAsync(_usage).ConfigureAwait(false);
            return 2;
        }

        try
        {
            var format = config["format"] ?? Path.GetExtension(input).TrimStart('.');
            var dataset = config["dataset"] ?? Path.GetFileNameWithoutExtension(input);
            var options = new LoadOptions
            {
                Duplicates = Enum.Parse<DuplicatePolicy>(config["duplicates"] ?? nameof(DuplicatePolicy.Skip), ignoreCase: true),
                StopOnError = config.GetValue<bool>("stop-on-error"),
                BatchSize = config.GetValue("batch-size", 1),
            };
            var sqlite = config["sqlite"];

            var services = new ServiceCollection();
            services.TryAddDataLoader();
            if (sqlite is null)
            {
                services.AddSingleton<IRowSink>(new InMemoryRowSink(key));
            }
            else
            {
                services.TryAddEntityFrameworkRowSink(dataset, key, (_, builder) => builder.UseSqlite($"Data Source={sqlite}"));
            }

            await using var provider = services.BuildServiceProvider();
            if (sqlite is not null)
            {
                await using var db = await provider.GetRequiredService<IDbContextFactory<LoadedRowsDbContext>>().CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                await db.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
            }

            await using var source = File.OpenRead(input);
            var report = await provider.GetRequiredService<ILoader>().LoadAsync(source, format, options, cancellationToken).ConfigureAwait(false);

            await output.WriteLineAsync($"{(sqlite is null ? "dry run" : "loaded")}: read {report.Read}, written {report.Written}, replaced {report.Replaced}, skipped {report.Skipped}, errors {report.Errors.Count}").ConfigureAwait(false);
            foreach (var rowError in report.Errors)
            {
                await error.WriteLineAsync($"{input}:{rowError.Line}: {rowError.Message}").ConfigureAwait(false);
            }

            return report.Errors.Any() ? 1 : 0;
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or FormatException or InvalidOperationException or UnauthorizedAccessException)
        {
            await error.WriteLineAsync(ex.Message).ConfigureAwait(false);
            return 2;
        }
    }
}
