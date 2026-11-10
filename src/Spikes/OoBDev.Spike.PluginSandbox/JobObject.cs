using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>A Windows job object: kills its processes when closed and caps each process's memory (phase 2 launcher proof).</summary>
[SupportedOSPlatform("windows")]
public sealed partial class JobObject : IDisposable
{
    private const int _extendedLimitInformation = 9;
    private const uint _killOnJobClose = 0x2000;
    private const uint _processMemory = 0x100;

    private readonly nint _handle;

    /// <summary>Creates the job with <paramref name="memoryMb"/> per process.</summary>
    public JobObject(int memoryMb)
    {
        _handle = CreateJobObjectW(0, null);
        var info = new ExtendedLimit { Basic = new BasicLimit { LimitFlags = _killOnJobClose | _processMemory }, ProcessMemoryLimit = (nuint)memoryMb * 1024 * 1024 };
        if (!SetInformationJobObject(_handle, _extendedLimitInformation, ref info, (uint)Marshal.SizeOf<ExtendedLimit>()))
        {
            throw new InvalidOperationException("SetInformationJobObject failed: " + Marshal.GetLastWin32Error());
        }
    }

    /// <summary>Puts <paramref name="process"/> in the job. A real launcher starts the process suspended first; here it has already run for a moment.</summary>
    public void Assign(Process process)
    {
        ArgumentNullException.ThrowIfNull(process);
        if (!AssignProcessToJobObject(_handle, process.Handle))
        {
            throw new InvalidOperationException("AssignProcessToJobObject failed: " + Marshal.GetLastWin32Error());
        }
    }

    /// <summary>Puts a process by raw handle in the job; used for a process created suspended.</summary>
    public void Assign(nint processHandle)
    {
        if (!AssignProcessToJobObject(_handle, processHandle))
        {
            throw new InvalidOperationException("AssignProcessToJobObject failed: " + Marshal.GetLastWin32Error());
        }
    }

    /// <inheritdoc />
    public void Dispose() => CloseHandle(_handle);

    [StructLayout(LayoutKind.Sequential)]
    private struct BasicLimit
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public nuint MinimumWorkingSetSize;
        public nuint MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public nuint Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IoCounters
    {
        public ulong A;
        public ulong B;
        public ulong C;
        public ulong D;
        public ulong E;
        public ulong F;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ExtendedLimit
    {
        public BasicLimit Basic;
        public IoCounters Io;
        public nuint ProcessMemoryLimit;
        public nuint JobMemoryLimit;
        public nuint PeakProcessMemoryUsed;
        public nuint PeakJobMemoryUsed;
    }

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial nint CreateJobObjectW(nint attributes, string? name);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetInformationJobObject(nint job, int infoClass, ref ExtendedLimit info, uint length);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AssignProcessToJobObject(nint job, nint process);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(nint handle);
}
