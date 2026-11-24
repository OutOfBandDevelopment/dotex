using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace OoBDev.System.Sandbox.Windows;

/// <summary>
/// Starts a process inside a Windows AppContainer with no capabilities, so it has no network access and can read and write
/// only what the container SID was granted, with an explicit environment block, only the three pipe handles inherited, and
/// the process created suspended so it is in its job object before its first instruction runs.
/// </summary>
[SupportedOSPlatform("windows")]
public static partial class AppContainerLauncher
{
    private const string _profileName = "OoBDev.System.Sandbox.Windows";
    private static readonly object _profileLock = new();
    private const uint _extendedStartupInfoPresent = 0x00080000;
    private const uint _createSuspended = 0x00000004;
    private const uint _createNoWindow = 0x08000000;
    private const uint _createUnicodeEnvironment = 0x00000400;
    private const uint _useStdHandles = 0x00000100;
    private const nint _securityCapabilitiesAttribute = 0x00020009;
    private const nint _handleListAttribute = 0x00020002;
    private const int _alreadyExists = unchecked((int)0x800700B7);

    /// <summary>The running plugin and the host ends of its three pipes.</summary>
    public sealed class Launched : IDisposable
    {
        /// <summary>The process id.</summary>
        public required int ProcessId { get; init; }

        /// <summary>The process handle (owned).</summary>
        public required SafeProcessHandle Process { get; init; }

        /// <summary>Writes to the plugin's standard input.</summary>
        public required Stream Input { get; init; }

        /// <summary>Reads the plugin's standard output.</summary>
        public required Stream Output { get; init; }

        /// <summary>Reads the plugin's standard error.</summary>
        public required Stream Error { get; init; }

        /// <summary>The AppContainer SID in string form.</summary>
        public required string ContainerSid { get; init; }

        /// <inheritdoc />
        public void Dispose()
        {
            Input.Dispose();
            Output.Dispose();
            Error.Dispose();
            Process.Dispose();
        }
    }

    /// <summary>The AppContainer SID of this spike's profile, creating the profile when it is missing.</summary>
    public static SecurityIdentifier ContainerSid()
    {
        int hr;
        nint sid;
        lock (_profileLock)
        {
            hr = CreateAppContainerProfile(_profileName, _profileName, "Plugin sandbox spike", 0, 0, out sid);
            if (hr == _alreadyExists)
            {
                hr = DeriveAppContainerSidFromAppContainerName(_profileName, out sid);
            }
        }

        if (hr != 0)
        {
            throw new Win32Exception(hr, "AppContainer profile failed");
        }

        try
        {
            return new SecurityIdentifier(sid);
        }
        finally
        {
            FreeSid(sid);
        }
    }

    /// <summary>Lets the container read and run everything under <paramref name="directory"/>, and optionally write to it.</summary>
    public static void Grant(string directory, bool write)
    {
        var info = new DirectoryInfo(directory);
        var security = info.GetAccessControl();
        var rights = write ? FileSystemRights.Modify : FileSystemRights.ReadAndExecute;
        security.AddAccessRule(new FileSystemAccessRule(ContainerSid(), rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
        info.SetAccessControl(security);
    }

    /// <summary>The name a host outside the container uses to reach a pipe the plugin created as <c>LOCAL\name</c>: an AppContainer can only create pipes in its own namespace.</summary>
    /// <param name="name">The pipe name without the <c>LOCAL\</c> prefix.</param>
    public static string PluginPipeName(string name) =>
        $"Sessions\\{Process.GetCurrentProcess().SessionId}\\AppContainerNamedObjects\\{ContainerSid().Value}\\{name}";

    /// <summary>Removes every access rule the container holds on <paramref name="directory"/> (what <see cref="Grant"/> added); returns true when a rule was removed. Call it when no running plugin needs the folder any more.</summary>
    public static bool Revoke(string directory)
    {
        var info = new DirectoryInfo(directory);
        var security = info.GetAccessControl();
        var sid = ContainerSid();
        var held = security.GetAccessRules(true, false, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>().Any(r => r.IdentityReference.Equals(sid));
        if (held)
        {
            security.PurgeAccessRules(sid);
            info.SetAccessControl(security);
        }

        return held;
    }

    /// <summary>Starts <paramref name="commandLine"/> in the container, suspended, assigns it to <paramref name="job"/>, then lets it run.</summary>
    public static Launched Start(string applicationPath, string commandLine, string workingDirectory, IReadOnlyDictionary<string, string> environment, JobObject job)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(job);
        var sid = ContainerSid();
        var sidBytes = new byte[sid.BinaryLength];
        sid.GetBinaryForm(sidBytes, 0);

        var security = new SecurityAttributes { Length = Marshal.SizeOf<SecurityAttributes>(), InheritHandle = 1 };
        MakePipe(out var childIn, out var hostIn, security, hostWrites: true);
        MakePipe(out var hostOut, out var childOut, security, hostWrites: false);
        MakePipe(out var hostErr, out var childErr, security, hostWrites: false);

        var sidMemory = Marshal.AllocHGlobal(sidBytes.Length);
        var capabilities = Marshal.AllocHGlobal(Marshal.SizeOf<SecurityCapabilities>());
        var handleList = Marshal.AllocHGlobal(3 * IntPtr.Size);
        var environmentBlock = Marshal.StringToHGlobalUni(string.Concat(environment.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase).Select(p => $"{p.Key}={p.Value}\0")) + "\0");
        nint attributeList = 0;
        try
        {
            Marshal.Copy(sidBytes, 0, sidMemory, sidBytes.Length);
            Marshal.StructureToPtr(new SecurityCapabilities { AppContainerSid = sidMemory }, capabilities, false);
            Marshal.WriteIntPtr(handleList, 0, childIn.DangerousGetHandle());
            Marshal.WriteIntPtr(handleList, IntPtr.Size, childOut.DangerousGetHandle());
            Marshal.WriteIntPtr(handleList, 2 * IntPtr.Size, childErr.DangerousGetHandle());

            nuint size = 0;
            InitializeProcThreadAttributeList(0, 2, 0, ref size);
            attributeList = Marshal.AllocHGlobal((nint)size);
            Check(InitializeProcThreadAttributeList(attributeList, 2, 0, ref size));
            Check(UpdateProcThreadAttribute(attributeList, 0, _securityCapabilitiesAttribute, capabilities, (nuint)Marshal.SizeOf<SecurityCapabilities>(), 0, 0));
            Check(UpdateProcThreadAttribute(attributeList, 0, _handleListAttribute, handleList, (nuint)(3 * IntPtr.Size), 0, 0));

            var startup = new StartupInfoEx
            {
                Info = new StartupInfo
                {
                    Size = Marshal.SizeOf<StartupInfoEx>(),
                    Flags = _useStdHandles,
                    StdInput = childIn.DangerousGetHandle(),
                    StdOutput = childOut.DangerousGetHandle(),
                    StdError = childErr.DangerousGetHandle(),
                },
                AttributeList = attributeList,
            };

            var line = Marshal.StringToHGlobalUni(commandLine);
            var started = CreateProcessW(applicationPath, line, 0, 0, true, _extendedStartupInfoPresent | _createSuspended | _createNoWindow | _createUnicodeEnvironment, environmentBlock, workingDirectory, ref startup, out var created);
            var startError = Marshal.GetLastWin32Error();
            Marshal.FreeHGlobal(line);
            if (!started)
            {
                throw new Win32Exception(startError);
            }
            try
            {
                job.Assign(created.Process);
            }
            catch
            {
                TerminateProcess(created.Process, 1);
                throw;
            }

            if (ResumeThread(created.Thread) == uint.MaxValue)
            {
                TerminateProcess(created.Process, 1);
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            CloseHandle(created.Thread);
            return new Launched
            {
                ProcessId = created.ProcessId,
                Process = new SafeProcessHandle(created.Process, true),
                Input = new FileStream(hostIn, FileAccess.Write, 1, false),
                Output = new FileStream(hostOut, FileAccess.Read, 1, false),
                Error = new FileStream(hostErr, FileAccess.Read, 1, false),
                ContainerSid = sid.Value,
            };
        }
        finally
        {
            childIn.Dispose();
            childOut.Dispose();
            childErr.Dispose();
            if (attributeList != 0)
            {
                DeleteProcThreadAttributeList(attributeList);
                Marshal.FreeHGlobal(attributeList);
            }

            Marshal.FreeHGlobal(sidMemory);
            Marshal.FreeHGlobal(capabilities);
            Marshal.FreeHGlobal(handleList);
            Marshal.FreeHGlobal(environmentBlock);
        }
    }

    private static void MakePipe(out SafeFileHandle read, out SafeFileHandle write, SecurityAttributes security, bool hostWrites)
    {
        Check(CreatePipe(out read, out write, ref security, 0));
        // The host's end must not be inherited; the child's end stays inheritable.
        Check(SetHandleInformation(hostWrites ? write : read, 1, 0));
    }

    private static void Check(bool success)
    {
        if (!success)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SecurityAttributes
    {
        public int Length;
        public nint Descriptor;
        public int InheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SecurityCapabilities
    {
        public nint AppContainerSid;
        public nint Capabilities;
        public uint CapabilityCount;
        public uint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct StartupInfo
    {
        public int Size;
        public nint Reserved;
        public nint Desktop;
        public nint Title;
        public uint X;
        public uint Y;
        public uint XSize;
        public uint YSize;
        public uint XCountChars;
        public uint YCountChars;
        public uint FillAttribute;
        public uint Flags;
        public ushort ShowWindow;
        public ushort Reserved2;
        public nint Reserved2Pointer;
        public nint StdInput;
        public nint StdOutput;
        public nint StdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct StartupInfoEx
    {
        public StartupInfo Info;
        public nint AttributeList;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public nint Process;
        public nint Thread;
        public int ProcessId;
        public int ThreadId;
    }

    [LibraryImport("userenv.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int CreateAppContainerProfile(string name, string displayName, string description, nint capabilities, uint capabilityCount, out nint sid);

    [LibraryImport("userenv.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int DeriveAppContainerSidFromAppContainerName(string name, out nint sid);

    [LibraryImport("advapi32.dll")]
    private static partial nint FreeSid(nint sid);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CreatePipe(out SafeFileHandle read, out SafeFileHandle write, ref SecurityAttributes attributes, uint size);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetHandleInformation(SafeFileHandle handle, uint mask, uint flags);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool InitializeProcThreadAttributeList(nint list, int count, uint flags, ref nuint size);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UpdateProcThreadAttribute(nint list, uint flags, nint attribute, nint value, nuint size, nint previous, nint returned);

    [LibraryImport("kernel32.dll")]
    private static partial void DeleteProcThreadAttributeList(nint list);

    [LibraryImport("kernel32.dll", EntryPoint = "CreateProcessW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CreateProcessW(string application, nint commandLine, nint processAttributes, nint threadAttributes, [MarshalAs(UnmanagedType.Bool)] bool inheritHandles, uint flags, nint environment, string directory, ref StartupInfoEx startup, out ProcessInformation information);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial uint ResumeThread(nint thread);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TerminateProcess(nint process, uint exitCode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(nint handle);
}
