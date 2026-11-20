using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>Wait, exit code, CPU time and kill on the process handle the launcher already owns, since <c>Process.GetProcessById</c> cannot read the exit code of a process it did not start.</summary>
[SupportedOSPlatform("windows")]
internal sealed partial class NativeProcess(SafeProcessHandle handle)
{
    private const uint _waitObject0 = 0;
    private const uint _stillActive = 259;

    public bool HasExited => ExitCode != _stillActive;

    public int ExitCode => GetExitCodeProcess(handle, out var code) ? (int)code : -1;

    public bool WaitForExit(int milliseconds) => WaitForSingleObject(handle, (uint)milliseconds) == _waitObject0;

    public TimeSpan TotalProcessorTime => GetProcessTimes(handle, out _, out _, out var kernel, out var user)
        ? TimeSpan.FromTicks(kernel + user)
        : TimeSpan.Zero;

    public void Kill() => TerminateProcess(handle, 1);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetExitCodeProcess(SafeProcessHandle process, out uint code);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial uint WaitForSingleObject(SafeProcessHandle process, uint milliseconds);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetProcessTimes(SafeProcessHandle process, out long creation, out long exit, out long kernel, out long user);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TerminateProcess(SafeProcessHandle process, uint exitCode);
}
