using System;
using System.Collections.Generic;

namespace OoBDev.System.Sandbox;

/// <summary>Starts plugins in the isolation of one platform (AppContainer and job object on Windows, bubblewrap on Linux, Seatbelt on macOS). The platform package registers its launcher; the application resolves it from DI.</summary>
public interface ISandboxLauncher
{
    /// <summary>The name of the isolation, for logs and test output.</summary>
    string Mechanism { get; }

    /// <summary>True when the isolation can run here (on Linux it needs bubblewrap installed).</summary>
    bool IsAvailable { get; }

    /// <summary>Starts the plugin with only the given folders reachable (read-only and read-write).</summary>
    /// <param name="commandArguments">The plugin command line, after the plugin assembly.</param>
    /// <param name="readDirectories">Folders the plugin may read.</param>
    /// <param name="writeDirectories">Folders the plugin may read and write.</param>
    /// <param name="limits">Process limits; null means the launcher's default.</param>
    /// <param name="surviveCaller">True lets the plugin outlive the process that started it (call <see cref="ISandboxedPlugin.Detach"/> to let go); only Windows supports it so far, the others throw <see cref="NotSupportedException"/>.</param>
    /// <returns>The running plugin.</returns>
    ISandboxedPlugin Start(string commandArguments, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null, JobLimits? limits = null, bool surviveCaller = false);
}
