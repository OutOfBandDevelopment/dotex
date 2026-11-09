using Microsoft.VisualStudio.TestTools.UnitTesting;
using global::System.Diagnostics;

namespace OoBDev.DevTerm.Tests;

/// <summary>Skips (Inconclusive) a test that runs a Python plugin when python isn't on the PATH.</summary>
internal static class PythonProbe
{
    public static void RequireOrSkip()
    {
        try
        {
            using var probe = Process.Start(new ProcessStartInfo("python", "--version") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false });
            if (probe is null || !probe.WaitForExit(15000) || probe.ExitCode != 0)
            {
                Assert.Inconclusive("python is not installed.");
            }
        }
        catch (global::System.ComponentModel.Win32Exception)
        {
            Assert.Inconclusive("python is not installed.");
        }    }
}
