using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace OoBDev.TestUtilities.Containers;

/// <summary>
/// Shared bookkeeping between test processes (one per test project) so a container is only stopped by the last process
/// using it, and only when the tests started it. Each process holds a lease file per container; a marker file says the tests
/// started the container. Leases of dead processes are ignored.
/// </summary>
internal sealed class ContainerLeases(string directory)
{
    private const string Marker = "started";

    /// <summary>The default location shared by all test processes of the user.</summary>
    public static string DefaultDirectory { get; } = Path.Combine(Path.GetTempPath(), "oobdev-container-leases");

    private string Folder(string container)
    {
        var folder = Path.Combine(directory, container);
        Directory.CreateDirectory(folder);
        return folder;
    }

    private Mutex Lock(string container)
    {
        var mutex = new Mutex(false, "oobdev-lease-" + container);
        try
        {
            mutex.WaitOne(TimeSpan.FromSeconds(10));
        }
        catch (AbandonedMutexException)
        {
        }

        return mutex;
    }

    /// <summary>Records that this process uses the container.</summary>
    public void Acquire(string container, bool startedByTests)
    {
        using var mutex = Lock(container);
        try
        {
            var folder = Folder(container);
            File.WriteAllText(Path.Combine(folder, Environment.ProcessId + ".lease"), string.Empty);
            if (startedByTests)
                File.WriteAllText(Path.Combine(folder, Marker), Environment.ProcessId.ToString(global::System.Globalization.CultureInfo.InvariantCulture));
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// Drops this process's lease. Returns true when the tests started the container and no other live process still uses it,
    /// so it is stopped by <paramref name="stop"/> while the lock is still held (a process that starts in between must not find it half stopped).
    /// </summary>
    public bool Release(string container, Action? stop = null)
    {
        using var mutex = Lock(container);
        try
        {
            var folder = Folder(container);
            File.Delete(Path.Combine(folder, Environment.ProcessId + ".lease"));
            var others = Directory.GetFiles(folder, "*.lease").Where(IsLive).ToList();
            foreach (var stale in Directory.GetFiles(folder, "*.lease").Except(others))
                File.Delete(stale);
            var marker = Path.Combine(folder, Marker);
            if (others.Count > 0 || !File.Exists(marker))
                return false;
            File.Delete(marker);
            stop?.Invoke();
            return true;
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private static bool IsLive(string leaseFile)
    {
        if (!int.TryParse(Path.GetFileNameWithoutExtension(leaseFile), out var pid))
            return false;
        try
        {
            using var process = Process.GetProcessById(pid);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
