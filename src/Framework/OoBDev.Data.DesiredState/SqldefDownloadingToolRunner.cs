using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Decorates a tool runner: a bare sqldef tool name that is not on <c>PATH</c> is fetched by <see cref="SqldefInstaller"/> first. A tool on <c>PATH</c> or a full path is used as given.
/// </summary>
public sealed class SqldefDownloadingToolRunner(IToolRunner inner, SqldefInstaller installer) : IToolRunner
{
    private static readonly HashSet<string> Tools = new(StringComparer.OrdinalIgnoreCase) { "psqldef", "sqlite3def", "mssqldef" };

    public async Task<ToolResult> RunAsync(string fileName, IReadOnlyList<string> arguments, string standardInput, IReadOnlyDictionary<string, string> environment, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        var resolved = Tools.Contains(fileName) && !OnPath(fileName)
            ? await installer.EnsureAsync(fileName, cancellationToken).ConfigureAwait(false)
            : fileName;
        return await inner.RunAsync(resolved, arguments, standardInput, environment, cancellationToken).ConfigureAwait(false);
    }

    private static bool OnPath(string tool)
    {
        var names = OperatingSystem.IsWindows() ? new[] { tool + ".exe", tool } : new[] { tool };
        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var name in names)
            {
                if (File.Exists(Path.Combine(directory, name)))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
