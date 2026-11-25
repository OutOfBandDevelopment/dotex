using System;
using System.IO;

namespace OoBDev.System.Sandbox;

/// <summary>An arbitrary program running in the isolation of one platform, spoken to over its own standard input and output (unlike <see cref="ISandboxedPlugin"/>, which speaks the frame protocol).</summary>
public interface ISandboxedProcess : IDisposable
{
    /// <summary>The program's standard input.</summary>
    Stream Input { get; }

    /// <summary>The program's standard output.</summary>
    Stream Output { get; }

    /// <summary>True once the program has ended.</summary>
    bool HasExited { get; }

    /// <summary>The exit code once it has ended.</summary>
    int ExitCode { get; }

    /// <summary>Which limit ended the program, or none.</summary>
    LimitHit LimitHit { get; }

    /// <summary>Ends the program and what it started.</summary>
    void Kill();

    /// <summary>Waits up to <paramref name="milliseconds"/> for the program to end.</summary>
    bool WaitForExit(int milliseconds);
}
