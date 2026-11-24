using System;
using System.Threading;

namespace OoBDev.System.Sandbox;

/// <summary>
/// Holds a cross-process named mutex for as long as it lives. A mutex belongs to the thread that took it, so a dedicated thread takes and releases it,
/// which lets the owner of a shared plugin dispose it from any thread.
/// </summary>
public sealed class InstanceHold : IDisposable
{
    private readonly ManualResetEventSlim _release = new(false);
    private Thread? _thread;

    private InstanceHold()
    {
    }

    /// <summary>True when the previous owner died without releasing the mutex, so its plugin may be left over.</summary>
    public bool Abandoned { get; private set; }

    /// <summary>Takes the mutex or returns null when another process holds it for longer than <paramref name="timeout"/>.</summary>
    public static InstanceHold? TryAcquire(string name, TimeSpan timeout)
    {
        var hold = new InstanceHold();
        var entered = false;
        using var ready = new ManualResetEventSlim(false);
        hold._thread = new Thread(() =>
        {
            using var entry = SharedInstanceLock.TryEnter(name, timeout);
            entered = entry is not null;
            hold.Abandoned = entry?.Abandoned ?? false;
            ready.Set();
            if (entry is not null)
            {
                hold._release.Wait();
            }
        })
        {
            IsBackground = true,
            Name = "plugin-instance-hold",
        };
        hold._thread.Start();
        ready.Wait();
        if (entered)
        {
            return hold;
        }

        hold._thread.Join();
        hold._release.Dispose();
        return null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _release.Set();
        _thread?.Join();
        _release.Dispose();
    }
}
