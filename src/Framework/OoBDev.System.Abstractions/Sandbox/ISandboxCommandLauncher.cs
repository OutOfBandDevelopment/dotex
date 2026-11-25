using System.Collections.Generic;

namespace OoBDev.System.Sandbox;

/// <summary>Starts any program, not only a plugin assembly, inside the same isolation as <see cref="ISandboxLauncher"/>.</summary>
public interface ISandboxCommandLauncher
{
    /// <summary>The name of the isolation, for logs and test output.</summary>
    string Mechanism { get; }

    /// <summary>True when the isolation can run here.</summary>
    bool IsAvailable { get; }

    /// <summary>Starts a program with only the given folders reachable (the program's own folder is not granted automatically; list it).</summary>
    /// <param name="fileName">The program, found on the system path when not rooted (Linux) or rooted (Windows).</param>
    /// <param name="arguments">The arguments, one entry each.</param>
    /// <param name="readDirectories">Folders the program may read.</param>
    /// <param name="writeDirectories">Folders the program may read and write.</param>
    /// <param name="limits">Process limits; null means the launcher's default.</param>
    /// <returns>The running program.</returns>
    ISandboxedProcess StartCommand(string fileName, IReadOnlyList<string> arguments, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null, JobLimits? limits = null);
}
