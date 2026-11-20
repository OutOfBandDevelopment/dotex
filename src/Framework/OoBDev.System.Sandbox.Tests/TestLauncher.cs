using System;
using System.Collections.Generic;
using OoBDev.System.Sandbox.Linux;
using OoBDev.System.Sandbox.MacOS;
using OoBDev.System.Sandbox.Windows;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>The launcher of the operating system the tests run on.</summary>
internal static class TestLauncher
{
    public static ISandboxLauncher Instance { get; } = OperatingSystem.IsWindows() ? new WindowsSandboxLauncher(TestPlugin.Dll)
        : OperatingSystem.IsLinux() ? new LinuxSandboxLauncher(TestPlugin.Dll)
        : new MacOSSandboxLauncher(TestPlugin.Dll);

    public static ISandboxedPlugin Start(string commandArguments, IReadOnlyCollection<string>? read = null, IReadOnlyCollection<string>? write = null) =>
        Instance.Start(commandArguments, read, write);
}
