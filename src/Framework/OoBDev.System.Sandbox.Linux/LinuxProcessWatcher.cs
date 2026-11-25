using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace OoBDev.System.Sandbox.Linux;

/// <summary>
/// Polls /proc for the plugin's process tree: counts the plugin's own processes (the bubblewrap supervisors and the shell are not counted),
/// sums CPU time and tracks peak memory. It enforces the process-count limit by killing the tree and lets the launcher classify why an
/// exit happened (CPU and memory limits are enforced by the kernel and the runtime, which only report an exit code).
/// </summary>
public sealed class LinuxProcessWatcher : IDisposable
{
    private static readonly HashSet<string> _infrastructure = new(StringComparer.Ordinal) { "bwrap", "sh", "bash", "dash" };
    private readonly int _rootPid;
    private readonly JobLimits _limits;
    private readonly Action<LimitHit> _exceeded;
    private readonly Thread _thread;
    private volatile bool _stop;
    private long _cpuTicks;
    private long _peakKilobytes;

    /// <summary>Starts watching the tree under <paramref name="rootPid"/>; <paramref name="exceeded"/> is called once, on the watcher thread, when the process-count limit is passed.</summary>
    public LinuxProcessWatcher(int rootPid, JobLimits limits, Action<LimitHit> exceeded)
    {
        _rootPid = rootPid;
        _limits = limits ?? throw new ArgumentNullException(nameof(limits));
        _exceeded = exceeded ?? throw new ArgumentNullException(nameof(exceeded));
        _thread = new Thread(Run) { IsBackground = true, Name = "plugin-proc-watcher" };
        _thread.Start();
    }

    /// <summary>Largest number of plugin processes seen at once.</summary>
    public int PeakProcesses { get; private set; }

    /// <summary>CPU seconds used by the tree, as last sampled.</summary>
    public double CpuSeconds => Interlocked.Read(ref _cpuTicks) / 100.0;

    /// <summary>Largest resident memory seen in the tree, in MB, summed over the processes at one sample.</summary>
    public double PeakMemoryMb => Interlocked.Read(ref _peakKilobytes) / 1024.0;

    /// <summary>Stops sampling.</summary>
    public void Dispose()
    {
        _stop = true;
        _thread.Join(1000);
    }

    internal static (int Parent, long Ticks, string Name)? Stat(int pid)
    {
        try
        {
            var text = File.ReadAllText($"/proc/{pid}/stat");
            var open = text.IndexOf('(', StringComparison.Ordinal);
            var close = text.LastIndexOf(')');
            var name = text[(open + 1)..close];
            var fields = text[(close + 2)..].Split(' ');
            return (int.Parse(fields[1], CultureInfo.InvariantCulture), long.Parse(fields[11], CultureInfo.InvariantCulture) + long.Parse(fields[12], CultureInfo.InvariantCulture), name);
        }
        catch (Exception ex) when (ex is IOException or FormatException or IndexOutOfRangeException or UnauthorizedAccessException or ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static long ResidentKilobytes(int pid)
    {
        try
        {
            foreach (var line in File.ReadLines($"/proc/{pid}/status"))
            {
                if (line.StartsWith("VmHWM:", StringComparison.Ordinal))
                {
                    return long.Parse(line["VmHWM:".Length..].Replace("kB", string.Empty, StringComparison.Ordinal).Trim(), CultureInfo.InvariantCulture);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or FormatException or UnauthorizedAccessException)
        {
        }

        return 0;
    }

    private void Run()
    {
        while (!_stop)
        {
            Sample();
            Thread.Sleep(50);
        }
    }

    private void Sample()
    {
        var parents = new Dictionary<int, (int Parent, long Ticks, string Name)>();
        foreach (var directory in Directory.EnumerateDirectories("/proc"))
        {
            if (int.TryParse(Path.GetFileName(directory), CultureInfo.InvariantCulture, out var pid) && Stat(pid) is { } stat)
            {
                parents[pid] = stat;
            }
        }

        var tree = new HashSet<int> { _rootPid };
        var grew = true;
        while (grew)
        {
            grew = false;
            foreach (var (pid, stat) in parents)
            {
                if (!tree.Contains(pid) && tree.Contains(stat.Parent))
                {
                    tree.Add(pid);
                    grew = true;
                }
            }
        }

        var count = 0;
        long ticks = 0;
        long kilobytes = 0;
        foreach (var pid in tree)
        {
            if (!parents.TryGetValue(pid, out var stat))
            {
                continue;
            }

            ticks += stat.Ticks;
            kilobytes += ResidentKilobytes(pid);
            if (!_infrastructure.Contains(stat.Name))
            {
                count++;
            }
        }

        Interlocked.Exchange(ref _cpuTicks, global::System.Math.Max(Interlocked.Read(ref _cpuTicks), ticks));
        Interlocked.Exchange(ref _peakKilobytes, global::System.Math.Max(Interlocked.Read(ref _peakKilobytes), kilobytes));
        PeakProcesses = global::System.Math.Max(PeakProcesses, count);
        if (_limits.MaxProcesses > 0 && count > _limits.MaxProcesses)
        {
            _stop = true;
            _exceeded(LimitHit.Processes);
        }
    }
}
