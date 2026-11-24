using Microsoft.VisualStudio.TestTools.UnitTesting;
using global::System.Diagnostics;

namespace OoBDev.DevTerm.Tests;

/// <summary>Ends a test Inconclusive when the runtime of an example plugin (python, java, go) is not on the PATH.</summary>
internal static class ToolProbe
{
    public static void RequireOrSkip(string command, string argument)
    {
        try
        {
            using var probe = Process.Start(new ProcessStartInfo(command, argument) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false });
            if (probe is null || !probe.WaitForExit(30000) || probe.ExitCode != 0)
            {
                Assert.Inconclusive($"{command} is not installed or does not run here.");
            }
        }
        catch (global::System.ComponentModel.Win32Exception)
        {
            Assert.Inconclusive($"{command} is not installed.");
        }
    }
}
