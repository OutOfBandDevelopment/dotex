using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;
using OoBDev.DevTerm.Logging;

namespace OoBDev.DevTerm.Configuration;

/// <summary>Applies the saved <see cref="AppPreferences.LogRetention"/> and <see cref="AppPreferences.ExportRetention"/> to the user's data folders.</summary>
public static class RetentionSweeper
{
    /// <summary>Runs both rules and returns every path removed. Call once at startup, before a new log or export is created.</summary>
    public static IReadOnlyList<string> Sweep(AppPreferences preferences, TimeProvider? timeProvider = null, string? logsDirectory = null, string? exportsDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        var at = (timeProvider ?? TimeProvider.System).GetUtcNow();
        var removed = new List<string>();
        removed.AddRange(preferences.LogRetention?.Apply(logsDirectory ?? DevTermUserDataPaths.LogsDirectory, "*" + SessionLogFormat.FileExtension, at) ?? []);
        removed.AddRange(preferences.ExportRetention?.Apply(exportsDirectory ?? DevTermUserDataPaths.ExportsDirectory, "*", at) ?? []);
        return removed;
    }
}
